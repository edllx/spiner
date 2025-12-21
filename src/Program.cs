using Spectre.Console;
using spinner;

using App app = new(string.Join(" ", args));

bool imageBuilt = false;
bool testsDone = false;

AnsiConsole.Write(new FigletText("Spinner").LeftJustified().Color(Color.Blue));

// Parse CLI command
// Parse input file
CLICommandOutput command = app.Init();

if (!command.Success)
{
    return;
}

var handleTaskFinished = (TaskResultBase result) =>
{
    switch (result.Tag)
    {
        case "ImageBuild":
            imageBuilt = true;
            break;

        case "TestRuns":
            testsDone = true;
            break;

        default:
            AnsiConsole.WriteLine($"Unknown tag {result.Tag}");
            app.Done = true;
            break;
    }
};

app.OnTaskDone += handleTaskFinished;

CancellationTokenSource imageBuildCancelationSource = new(TimeSpan.FromMinutes(2));
CancellationTokenSource testRunCancelationSource = new(TimeSpan.FromMinutes(5));

await AnsiConsole
    .Status()
    .SpinnerStyle(Style.Parse("green bold"))
    .StartAsync(
        "Building Images",
        async ctx =>
        {
            await app.BuildImages();
            while (!imageBuilt && !imageBuildCancelationSource.Token.IsCancellationRequested)
            {
                await Task.Delay(250);
            }
            if (imageBuildCancelationSource.Token.IsCancellationRequested)
            {
                app.Logger.Log("Image building Timeout", LogLevel.Warning);
            }

            ctx.Status("Running tests");

            await app.RunTests();
            while (!testsDone && !imageBuildCancelationSource.Token.IsCancellationRequested)
            {
                await Task.Delay(250);
            }
            if (testRunCancelationSource.Token.IsCancellationRequested)
            {
                app.Logger.Log("Test runs timeout", LogLevel.Warning);
            }
        }
    );

app.OnTaskDone -= handleTaskFinished;

if (app.Results is null)
{
    return;
}
AnsiConsole.Write(app.Results.Format() ?? new Markup(""));
