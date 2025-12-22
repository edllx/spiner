using System.Text;

namespace spinner;

public partial class App
{
    private string GetBuildContextFromPath(string buildPath)
    {
        var parts = buildPath.Split("/");
        StringBuilder b = new();

        for (int j = 1; j < parts.Length - 1; j++)
        {
            b.Append($"/{parts[j]}");
        }
        return b.ToString();
    }

    private void AddImageTasks(TaskSequence sequence)
    {
        TaskBatch imageBatch = new() { Tag = "ImageBatch" };

        for (int i = 0; i < ServiceManager.Templates.Count; i++)
        {
            var template = ServiceManager.Templates[i];

            if (string.IsNullOrEmpty(template.BuildPath))
            {
                continue;
            }

            var ctx = GetBuildContextFromPath(template.BuildPath);

            imageBatch.Add(async () =>
            {
                try
                {
                    bool imageExist = await _podman.ImageExist(template.ImageName);
                    if (imageExist && !ImageRebuild)
                    {
                        Logger.Log(
                            $"Image {template.ImageName} exist and image-rebuid disabled",
                            logLevel: LogLevel.Debug
                        );

                        return new();
                    }

                    if (imageExist)
                    {
                        Logger.Log(
                            $"Rebuilding Image {template.ImageName}",
                            logLevel: LogLevel.Info
                        );
                    }
                    else
                    {
                        Logger.Log($"Building Image {template.ImageName}", logLevel: LogLevel.Info);
                    }

                    await _podman.BuildImageAsync(
                        buildFilePath: template.BuildPath,
                        context: ctx,
                        tag: template.ImageName
                    );

                    return new();
                }
                catch (Exception ex)
                {
                    Logger.Log(
                        $"Image {template.ImageName} Creation failed\n{ex.Message}",
                        logLevel: LogLevel.Warning
                    );

                    return new() { Success = false, Error = ex.Message };
                }
            });
        }

        sequence.Add(async () =>
        {
            await imageBatch.Run();
            return new();
        });

        sequence.Add(async () =>
        {
            await _podman.ImagePune();
            return new();
        });
    }
}
