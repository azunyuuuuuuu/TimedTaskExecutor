namespace TimedTaskExecutor.Models;

public record TaskDefinition
{
    public string Command { get; init; }
    public string Parameter { get; init; }
    public string Path { get; init; }
}