using TimedTaskExecutor;
using TimedTaskExecutor.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<ExecutionService>();
    })
    .Build();

await host.RunAsync();
