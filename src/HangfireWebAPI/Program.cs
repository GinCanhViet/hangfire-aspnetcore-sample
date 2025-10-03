using Hangfire;
using Hangfire.Console;
using Hangfire.RecurringJobAdmin;
using Hangfire.SqlServer;

namespace HangfireWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = builder.Configuration;
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();

            ConfigureHangfire(builder, connectionString);

            builder.Services.AddTransient<BusinessLogic.IDateTimeJobLogic, BusinessLogic.DateTimeJobLogic>();
            builder.Services.AddTransient<Jobs.DateTimeJobLogicJobRunner>();

            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseHangfireDashboard("/hangfire", ConfigureHangfireDashboard());

            app.Run();
        }

        private static void ConfigureHangfire(WebApplicationBuilder builder, string connectionString)
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

            builder.Services.AddHangfire(
                configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseSqlServerStorage(connectionString, sqlServerOptions)
                .WithJobExpirationTimeout(TimeSpan.FromDays(1))
                .UseConsole(new ConsoleOptions()
                {
                    ExpireIn = TimeSpan.FromHours(7),
                    PollInterval = 5000
                })
                .UseRecurringJobAdmin(
                    typeof(Jobs.DateTimeJobLogicJobRunner).Assembly, 
                    typeof(Hangfire.Server.PerformContext).Assembly
                )
            );
        }

        private static DashboardOptions ConfigureHangfireDashboard()
        {
            return new DashboardOptions()
            {
                DarkModeEnabled = true,
                StatsPollingInterval = 2000,
                DashboardTitle = "Hangfire Dashboard",
                DisplayStorageConnectionString = false
            };
        }
    }
}
