namespace spinner;

public partial class App
{
    private BaseTask CreateTests(Tests tests, int port)
    {
        switch (tests.Mode)
        {
            case "sync":
                TaskSequence sequence = new();
                for (int i = 0; i < tests.TestSet.Length; i++)
                {
                    var f = HandleRequest(port, tests.TestSet[i], this);
                    sequence.Add(f);
                }

                return sequence;

            default:
                TaskBatch batch = new();
                for (int i = 0; i < tests.TestSet.Length; i++)
                {
                    var f = HandleRequest(port, tests.TestSet[i], this);
                    batch.Add(f);
                }
                return batch;
        }
    }

    private static Func<Task<TaskResult>> HandleRequest(int port, Test test, App app)
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

            var resolvedPath = KeyManager.Resolve(path, test.Scope);

            HttpResponse? response = null;
            try
            {
                app.Logger.Log($"{id}:{method}: localhost:{port}/{resolvedPath}");
                switch (method)
                {
                    case "POST":
                        if (app.Debug && test.Request is not null && test.Request.Body is not null)
                        {
                            app.Logger.Log($"\n{test.Request.Body.ToString(0)}", LogLevel.Debug);
                        }

                        response = await contex.Post(resolvedPath, body);
                        break;

                    case "PATCH":
                        if (app.Debug && test.Request is not null && test.Request.Body is not null)
                        {
                            app.Logger.Log($"\n{test.Request.Body.ToString(0)}", LogLevel.Debug);
                        }
                        response = await contex.Patch(resolvedPath, body);
                        break;

                    case "PUT":
                        if (app.Debug && test.Request is not null && test.Request.Body is not null)
                        {
                            app.Logger.Log($"\n{test.Request.Body.ToString(0)}", LogLevel.Debug);
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
                            app.Logger.Log(
                                $"{id}:Set: Did not found a value for {item.Value}",
                                LogLevel.Warning
                            );
                        }
                        else
                        {
                            test.Scope.Set(item.Key, valueR.Value);
                        }
                    }

                    foreach (var item in test.Asserts?.Asserts ?? [])
                    {
                        bool result = false;
                        switch (item)
                        {
                            case AssertEquals eq:
                                var eqq = new AssertEquals(
                                    response.JsonFind(eq.Exptected, test.Scope).Value,
                                    response.JsonFind(eq.Actual, test.Scope).Value
                                );
                                result = eqq.evaluate().Success;

                                app.Logger.Log(
                                    $"{id}:Assert: {eq.Actual} == {eq.Exptected} {(result ? $"{AnsiColors.Colorize("Success", AnsiColors.Green)}" : $"{AnsiColors.Colorize("Failed", AnsiColors.Red)} Found: {eqq.Actual}")}"
                                );

                                break;
                            case AssertNotNull ntn:
                                var ntnValue = test.Scope.Get(ntn.Key);
                                var isEmpty = string.IsNullOrEmpty(ntnValue);

                                app.Logger.Log(
                                    $"{id}:Assert: {ntn.Key} NOT NULL {(!isEmpty ? $"{AnsiColors.Colorize("Success", AnsiColors.Green)} Found: {ntnValue}" : $"{AnsiColors.Colorize("Failed", AnsiColors.Red)}")}"
                                );

                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                app.Logger.Log($"Something failed: {ex.Message}");
            }
            finally
            {
                if (response is not null)
                {
                    response.Dispose();
                }
            }
            return new();
        };
    }
}
