using System.Text;
using Spectre.Console;
using Spectre.Console.Json;

namespace spinner;

public partial class App
{
    private BaseTask CreateTests(Tests tests, int port, TestResultTree tree)
    {
        switch (tests.Mode)
        {
            case "sync":
                TaskSequence sequence = new();
                for (int i = 0; i < tests.TestSet.Length; i++)
                {
                    var f = HandleRequest(port, tests.TestSet[i], tree);
                    sequence.Add(f);
                }

                return sequence;

            default:
                TaskBatch batch = new();
                for (int i = 0; i < tests.TestSet.Length; i++)
                {
                    var f = HandleRequest(port, tests.TestSet[i], tree);
                    batch.Add(f);
                }
                return batch;
        }
    }

    private Func<Task<TaskResult>> HandleRequest(int port, Test test, TestResultTree tree)
    {
        return async () =>
        {
            using var contex = new HttpContext(new() { BaseUri = $"http://localhost:{port}" });

            if (test.Request is not null && test.Request.Body is not null)
            {
                test.Request.Body.Resolve(test.Scope);
            }

            if (test.Request is not null && test.Asserts is not null)
            {
                test.Asserts.Resolve(test.Scope);
            }

            var method = test.Request?.Method ?? "GET";
            var path = test.Request?.Path ?? "";
            var body = test.Request?.Body?.Model();
            var id = Tools.GenerateRandomString(12, "Test-");
            string description = test.Description;

            if (string.IsNullOrEmpty(description))
            {
                description = id;
            }

            var resolvedPath = KeyManager.Resolve(path, test.Scope);

            HttpResponse? response = null;
            try
            {
                Logger.Log(
                    $"{description} :{AnsiColors.Colorize($"{method}: /{resolvedPath}", AnsiColors.Info)}"
                );
                switch (method)
                {
                    case "POST":
                        if (test.Request is not null && test.Request.Body is not null)
                        {
                            Logger.Log($"\n{test.Request.Body.ToString(0)}", LogLevel.Debug);
                        }

                        response = await contex.Post(resolvedPath, body);
                        break;

                    case "PATCH":
                        if (test.Request is not null && test.Request.Body is not null)
                        {
                            Logger.Log($"\n{test.Request.Body.ToString(0)}", LogLevel.Debug);
                        }
                        response = await contex.Patch(resolvedPath, body);
                        break;

                    case "PUT":
                        if (test.Request is not null && test.Request.Body is not null)
                        {
                            Logger.Log($"\n{test.Request.Body.ToString(0)}", LogLevel.Debug);
                        }
                        response = await contex.Put(resolvedPath, body);
                        break;

                    // GET
                    default:
                        response = await contex.Get(resolvedPath);
                        break;
                }

                if (response is not null)
                {
                    foreach (var item in test.Response?.Setters ?? [])
                    {
                        var valueR = response.JsonFind(item.Value, test.Scope);
                        if (!valueR.Found)
                        {
                            Logger.Log(
                                $"{description} :Set: Did not found a value for {item.Value}",
                                LogLevel.Warning
                            );
                        }
                        else
                        {
                            test.Scope.Set(item.Key, valueR.Value);
                        }
                    }

                    if (test.Asserts is null || test.Asserts.Asserts is null)
                    {
                        return new();
                    }

                    bool result = false;

                    for (int i = 0; i < test.Asserts!.Asserts!.Length; i++)
                    {
                        var t = test.Asserts.Asserts[i];
                        StringBuilder b = new();

                        switch (t)
                        {
                            case AssertEquals eq:
                                var eqq = new AssertEquals(
                                    response.JsonFind(eq.Exptected, test.Scope).Value,
                                    response.JsonFind(eq.Actual, test.Scope).Value
                                );
                                result = eqq.evaluate().Success;

                                Logger.Log(
                                    $"{description}:Assert: {eq.Actual} == {eq.Exptected} {(result ? $"{AnsiColors.Colorize("Success", AnsiColors.Green)}" : $"{AnsiColors.Colorize("Failed", AnsiColors.Red)} Found: {eqq.Actual}")}",
                                    LogLevel.Debug
                                );

                                if (!result)
                                {
                                    if (
                                        test.Asserts is not null
                                        && test.Asserts.Asserts is not null
                                    )
                                    {
                                        b.Append(
                                            $"[red]Failed {eqq.ToString(0).EscapeMarkup()}[/]"
                                        );
                                    }
                                }

                                break;
                            case AssertNotNull ntn:
                                var ntnValue = test.Scope.Get(ntn.Key);
                                var isEmpty = string.IsNullOrEmpty(ntnValue);

                                result = !isEmpty;

                                Logger.Log(
                                    $"{description}:Assert: {ntn.Key} NOT NULL {(!isEmpty ? $"{AnsiColors.Colorize("Success", AnsiColors.Green)} Found: {ntnValue}" : $"{AnsiColors.Colorize("Failed", AnsiColors.Red)}")}",
                                    LogLevel.Debug
                                );

                                if (!result)
                                {
                                    if (
                                        test.Asserts is not null
                                        && test.Asserts.Asserts is not null
                                    )
                                    {
                                        b.Append(
                                            $"[red]Failed {ntn.ToString(0).EscapeMarkup()}[/]"
                                        );
                                    }
                                }

                                break;
                            default:
                                break;
                        }

                        if (!result)
                        {
                            if (test.Request is not null)
                            {
                                b.Append(
                                    $"\n[purple]{test.Request?.ToString(0).EscapeMarkup()}[/]"
                                );
                            }

                            if (test.Scope.Parent is not null)
                            {
                                b.Append(
                                    $"\n[blue]{test.Scope.Combine(test.Scope.Parent).ToString(0).EscapeMarkup()}[/]"
                                );
                            }

                            var json = response.Document?.RootElement.ToString() ?? "{}";
                            var jsonText = new JsonText(json);

                            var p1 = new Markup(
                                $"{b.ToString()}\nResponse:\n{response.StatusCode}\n"
                            );
                            var p2 = new Panel(jsonText).NoBorder();

                            tree.Branches.Add(
                                new TestResultLeaf($"{description}", false, new Rows(p1, p2))
                            );
                            return new() { Success = false };
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log($"Something failed: {ex.Message}");
            }
            finally
            {
                if (response is not null)
                {
                    response.Dispose();
                }
            }

            tree.Branches.Add(new TestResultLeaf($"{description}", true));
            return new();
        };
    }
}
