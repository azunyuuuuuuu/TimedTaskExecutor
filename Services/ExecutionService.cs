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
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var task in _tasks)
            {
                var expression = CronExpression.Parse(task.Schedule);
                var next = expression.GetNextOccurrence(task.LastRuntime, TimeZoneInfo.Local);

                if (next < DateTimeOffset.Now)
                {
                    _logger.LogTrace($"Executing Task {task}");
                    task.LastRuntime = DateTimeOffset.Now;
                }
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
