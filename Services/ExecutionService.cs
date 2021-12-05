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
    private List<TaskDefinition>? _tasks = new List<TaskDefinition>();

    public ExecutionService(IHostApplicationLifetime lifetime, ILogger<ExecutionService> logger, IConfiguration config)
    {
        _lifetime = lifetime;
        _logger = logger;
        _config = config;

        InitializeConfiguration();
    }

    private void InitializeConfiguration()
    {
        var _tasks = _config.GetSection("Tasks").Get<List<TaskDefinition>>();
        if (_tasks == null || _tasks.Count == 0)
        {
            _logger.LogError("No tasks defined");
            _lifetime.StopApplication();
        }
        else
        {
            _logger.LogInformation($"{_tasks.Count} tasks found");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(3000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var task in _tasks ?? new List<TaskDefinition>())
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
