namespace TimedTaskExecutor.Services;

public class ExecutionService : BackgroundService
{
    private readonly ILogger<ExecutionService> _logger;

    public ExecutionService(ILogger<ExecutionService> logger)
    {
        _logger = logger;
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
