using Google.Apis.Indexing.v3.Data;

public class GoogleNotificationRequest
{
    public Guid TaskId { get; set; }
    public UrlNotification Notification { get; set; }

    public GoogleNotificationRequest(Guid taskId, UrlNotification notification)
    {
        TaskId = taskId;
        Notification = notification;
    }
}