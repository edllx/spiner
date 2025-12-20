using System.Text;
using Spectre.Console;

namespace spinner;

[Flags]
public enum LoggerOutput
{
    Stdout,
    File,
}

public enum LogLevel
{
    Info,
    Debug,
    Warning,
    Error,
    Critial,
}

public struct LogMessage
{
    DateTime Time { get; init; }
    public LogLevel LogLevel { get; init; }
    public string Message { get; init; }

    public LogMessage(string message, LogLevel logLevel = LogLevel.Info)
    {
        LogLevel = logLevel;
        Message = message;
        Time = DateTime.Now;
    }

    public override string ToString()
    {
        switch (LogLevel)
        {
            case LogLevel.Critial:
                return AnsiColors.Colorize($"[{LogLevel}]: {Message}", AnsiColors.Red);

            case LogLevel.Error:
                return AnsiColors.Colorize($"[{LogLevel}]: {Message}", AnsiColors.Error);
            case LogLevel.Warning:
                return AnsiColors.Colorize($"[{LogLevel}]: {Message}", AnsiColors.Yellow);
        }
        return $"[{LogLevel}]: {Message}";
    }
}

public class Logger
{
    public LoggerOutput Output { get; } = LoggerOutput.Stdout | LoggerOutput.File;
    public string OutputFile { get; init; }
    public event EventHandler<LogMessage>? OnMessageReceived;
    private OwnedSemaphore _lock = new(1, 1);

    public Logger()
    {
        OutputFile = $"testOutputs/sp-output-{Tools.GenerateRandomString(8)}.txt";
    }

    public Logger(string outputFile)
    {
        OutputFile = outputFile;
    }

    private void Register(LogMessage message, object source)
    {
        if (Output.HasFlag(LoggerOutput.File))
        {
            _ = WriteToFile(message);
        }

        if (Output.HasFlag(LoggerOutput.Stdout))
        {
            AnsiConsole.WriteLine(message.ToString());
        }

        if (OnMessageReceived is null)
        {
            return;
        }

        OnMessageReceived.Invoke(source, message);
    }

    private async Task WriteToFile(LogMessage message)
    {
        int randomId = Random.Shared.Next(1000, 10000);
        await _lock.WaitAsync(randomId);
        try
        {
            byte[] content = Encoding.UTF8.GetBytes($"{message.ToString()}\n");
            using FileStream stream = new(OutputFile, FileMode.Append, FileAccess.Write);
            await stream.WriteAsync(content);
        }
        catch (Exception) { }
        finally
        {
            _lock.Release(randomId);
        }
    }

    public void Log(string message, LogLevel logLevel = LogLevel.Info, object? source = null)
    {
        source = source ?? this;
        Register(new(message, logLevel: logLevel), source: source);
    }
}
