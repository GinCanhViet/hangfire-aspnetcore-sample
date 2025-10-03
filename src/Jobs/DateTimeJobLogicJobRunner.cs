using BusinessLogic;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace Jobs
{
    public class DateTimeJobLogicJobRunner
    {
        private readonly ILogger<DateTimeJobLogicJobRunner> _logger;
        private readonly IDateTimeJobLogic _logic;

        public DateTimeJobLogicJobRunner(ILogger<DateTimeJobLogicJobRunner> logger, IDateTimeJobLogic logic)
        {
            _logger = logger;
            _logic = logic;
        }

        public async Task LogCurrentTimeJob(PerformContext context)
        {
            var stringDateTimeNow = _logic.GetCurrentTimeString();
            context?.WriteLine($"[{stringDateTimeNow}] Starting HangFire Recurring Job...");

            await _logic.SimulateLongRunningTaskAsync();

            context?.WriteLine($"[{stringDateTimeNow}] Job finished successfully. Check application log for details.");

            _logger.LogInformation("HangFire Recurring Job is running successfully at: {time}", stringDateTimeNow);
        }
    }
}
