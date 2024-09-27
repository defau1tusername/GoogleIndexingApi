using Google.Apis.Indexing.v3.Data;

public class UrlNotificationBuilder
{
    public UrlNotification GetUpdate(Uri url) =>
        new() { Type = "URL_UPDATED", Url = url.ToString() };

    public UrlNotification GetDelete(Uri url) =>
        new() { Type = "URL_DELETED", Url = url.ToString() };
}