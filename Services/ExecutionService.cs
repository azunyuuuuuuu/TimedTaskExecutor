using CliWrap;
using CliWrap.Buffered;
using Cronos;
using TimedTaskExecutor.Models;

namespace TimedTaskExecutor.Services;

public class ExecutionService : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<ExecutionService> _logger;
    private readonly IConfiguration _config;
    private List<TaskDefinition> _tasks = new List<TaskDefinition>();

    public ExecutionService(IHostApplicationLifetime lifetime, ILogger<ExecutionService> logger, IConfiguration config)
    {
        _lifetime = lifetime;
        _logger = logger;
        _config = config;

        InitializeConfiguration();
    }

    private void InitializeConfiguration()
    {
        var temp = _config.GetSection("Tasks").Get<List<TaskDefinition>>();

        if (temp == null || temp.Count == 0)
        {
            _logger.LogError("No tasks defined");
            _lifetime.StopApplication();
        }
        else
        {
            _tasks.AddRange(temp);
            _logger.LogInformation($"{temp.Count} tasks found");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(3000, stoppingToken);
        _logger.LogInformation("Start processing");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogTrace($"Processing loop start at {DateTimeOffset.Now}");

            foreach (var task in _tasks)
            {
                _logger.LogTrace($"Checking task {task}");
                if (task.NextRuntime >= DateTimeOffset.Now)
                    continue;

                _logger.LogTrace($"Processing task {task}");

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

            _logger.LogTrace($"Processing loop end at {DateTimeOffset.Now}");
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("End processing");
    }
}
