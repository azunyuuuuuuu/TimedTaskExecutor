namespace TimedTaskExecutor.Models;

public record TaskDefinition
{
    public string ExecutablePath { get; init; }
    public string Arguments { get; init; }
    public string WorkingDirectory { get; init; }
    public string Schedule { get; init; } = "* * * * *";
    public DateTimeOffset NextRuntime { get; set; } = DateTimeOffset.MinValue;
}