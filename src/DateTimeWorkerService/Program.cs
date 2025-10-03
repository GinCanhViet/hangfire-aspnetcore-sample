using Hangfire;
using Hangfire.Console;
using Hangfire.RecurringJobAdmin;
using Hangfire.SqlServer;

namespace DateTimeWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            var configuration = builder.Configuration;
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            ConfigureHangfire(builder, connectionString);

            builder.Services.AddTransient<BusinessLogic.IDateTimeJobLogic, BusinessLogic.DateTimeJobLogic>();
            builder.Services.AddTransient<Jobs.DateTimeJobLogicJobRunner>();
            builder.Services.AddHostedService<InitialJobRunner>();          

            var host = builder.Build();
            
            host.Run();
        }

        private static void ConfigureHangfire(HostApplicationBuilder builder, string connectionString)
        {
            var sqlServerOptions = new SqlServerStorageOptions
            {
                PrepareSchemaIfNecessary = true,
                DisableGlobalLocks = true,
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(15),
                CommandTimeout = TimeSpan.FromMinutes(5),
                UseRecommendedIsolationLevel = true,
                JobExpirationCheckInterval = TimeSpan.FromHours(1)
            };

            builder.Services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseSqlServerStorage(connectionString, sqlServerOptions)
                .WithJobExpirationTimeout(TimeSpan.FromDays(1))
                .UseConsole(new ConsoleOptions
                {
                    ExpireIn = TimeSpan.FromHours(7),
                    PollInterval = 5000
                })
                .UseRecurringJobAdmin(typeof(Jobs.DateTimeJobLogicJobRunner).Assembly, typeof(Hangfire.Server.PerformContext).Assembly)
            );

            builder.Services.AddHangfireServer(options =>
            {
                options.ServerName = "DateTimeWorkerServer";
                options.WorkerCount = Environment.ProcessorCount;
            });
        }
    }

    public class InitialJobRunner : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Enqueue the job to run immediately on startup
            BackgroundJob.Enqueue<Jobs.DateTimeJobLogicJobRunner>(jobRunner => jobRunner.LogCurrentTimeJob(null));

            // Schedule the recurring job to run every minute
            RecurringJob.AddOrUpdate<Jobs.DateTimeJobLogicJobRunner>(
                "Log-Current-Time-Job",
                jobRunner => jobRunner.LogCurrentTimeJob(null),
                "*/1 * * * *",
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
                }
            );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
