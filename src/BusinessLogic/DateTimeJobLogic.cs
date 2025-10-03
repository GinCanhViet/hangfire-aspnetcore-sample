namespace BusinessLogic
{
    public interface IDateTimeJobLogic
    {
        string GetCurrentTimeString();
        Task SimulateLongRunningTaskAsync();
    }

    public class DateTimeJobLogic : IDateTimeJobLogic
    {
        public string GetCurrentTimeString()
        {
            return DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
        }

        public async Task SimulateLongRunningTaskAsync()
        {
            await Task.Delay(500);
        }
    }
}
