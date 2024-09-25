using Google.Apis.Indexing.v3.Data;

public class TaskExecutorBgService : BackgroundService
{
    private readonly ServiceAccountsAccessor serviceAccountsAccessor;
    private readonly GoogleTasksAccessor googleTasksAccessor;

    public TaskExecutorBgService(
        ServiceAccountsAccessor serviceAccountsAccessor,
        GoogleTasksAccessor googleTasksAccessor)
    {
        this.serviceAccountsAccessor = serviceAccountsAccessor;
        this.googleTasksAccessor = googleTasksAccessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timeOfSecondQueue = DateTime.Today.AddHours(23);
        while (!stoppingToken.IsCancellationRequested)
        {
            if (DateTime.Now >= timeOfSecondQueue)
            {
                await ExecuteTasksAsync(true);
                timeOfSecondQueue = timeOfSecondQueue.AddDays(1);
            }
            else
                await ExecuteTasksAsync();

            await Task.Delay(100);
        }
    }

    private async Task ExecuteTasksAsync(bool isTimeOfSecondQueue = false)
    {
        var serviceAccount = await serviceAccountsAccessor.GetAccountAsync();
        if (serviceAccount == null)
            return;

        var tasks = isTimeOfSecondQueue
            ? await googleTasksAccessor.GetTasksAsync(int.Min(serviceAccount.QuotaCount, 100))
            : await googleTasksAccessor.GetTasksAsync(int.Min(serviceAccount.QuotaCount, 100), true);

        if (!tasks.Any())
            return;

        var googleNotificationRequests = GetGoogleNotificationRequests(tasks);

        var googleClient = new GoogleClient(serviceAccount.Key);
        var googleResponses = await googleClient.SendNotificationsAsync(googleNotificationRequests);

        await serviceAccountsAccessor.ReduceQuotaAsync(serviceAccount, tasks.Count);
        await googleTasksAccessor.UpdateGoogleResponsesAsync(googleResponses);
    }

    private List<GoogleNotificationRequest> GetGoogleNotificationRequests(List<GoogleTask> tasks)
    {
        var googleNotificationRequests = new List<GoogleNotificationRequest>();
        var urlNotificationBuilder = new UrlNotificationBuilder();

        foreach (var task in tasks)
        {
            var notification = new UrlNotification();
            switch (task.UrlAction)
            {
                case GoogleTask.Action.Update:
                    notification = urlNotificationBuilder.GetUpdate(task.Url);
                    break;

                case GoogleTask.Action.Delete:
                    notification = urlNotificationBuilder.GetDelete(task.Url);
                    break;
            }
            var googleNotificationRequest = new GoogleNotificationRequest(task.Id, notification);
            googleNotificationRequests.Add(googleNotificationRequest);
        }

        return googleNotificationRequests;
    }
}