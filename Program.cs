using TimedTaskExecutor;
using TimedTaskExecutor.Services;

string AppDataBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(TimedTaskExecutor));

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLogging(builder =>
        {
            builder.AddFile(Path.Combine(AppDataBasePath, "logs", "app_{0:yyyy}-{0:MM}-{0:dd}.log"), fileLoggerOpts =>
            {
                fileLoggerOpts.FormatLogFileName = fName => { return String.Format(fName, DateTime.UtcNow); };
            });
        });
        services.AddHostedService<ExecutionService>();
    })
    .Build();

await host.RunAsync();
