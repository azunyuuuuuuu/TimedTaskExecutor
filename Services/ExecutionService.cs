using TimedTaskExecutor.Models;

namespace TimedTaskExecutor.Services;

public class ExecutionService : BackgroundService
{
    private readonly ILogger<ExecutionService> _logger;
    private readonly IConfiguration _config;

    public ExecutionService(ILogger<ExecutionService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;

        InitializeConfiguration();
    }

    private void InitializeConfiguration()
    {
        var tasks = _config.GetSection("Tasks").Get<List<TaskDefinition>>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
