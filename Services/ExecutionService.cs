using CliWrap;
using CliWrap.Buffered;
using Cronos;
using TimedTaskExecutor.Models;

namespace TimedTaskExecutor.Services;

public class ExecutionService : BackgroundService
{
    private readonly ILogger<ExecutionService> _logger;
    private readonly IConfiguration _config;
    private List<TaskDefinition> _tasks = new List<TaskDefinition>();

    public ExecutionService(ILogger<ExecutionService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;

        InitializeConfiguration();
    }

    private void InitializeConfiguration()
    {
        _tasks = _config.GetSection("Tasks").Get<List<TaskDefinition>>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(3000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var task in _tasks)
            {
                if (task.NextRuntime < DateTimeOffset.Now)
                {
                    _logger.LogTrace($"Executing Task {task}");

                    var expression = CronExpression.Parse(task.Schedule);
                    task.NextRuntime = expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local) ?? DateTimeOffset.Now;

                    try
                    {
                        var results = await Cli.Wrap(task.ExecutablePath)
                            .WithArguments(task.Arguments)
                            .WithWorkingDirectory(task.WorkingDirectory)
                            .ExecuteBufferedAsync(stoppingToken);

                        _logger.LogTrace($"Task completed successfully {task}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error during execution of {task}");
                    }
                }
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
