using Google.Apis.Auth.OAuth2;
using Google.Apis.Indexing.v3;
using Google.Apis.Indexing.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Services;

public class GoogleClient
{
    private readonly IndexingService googleService;

    public GoogleClient(string apiKey)
    {
        var credential = GoogleCredential.FromJson(apiKey)
            .CreateScoped(IndexingService.Scope.Indexing);

        googleService = new IndexingService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
        });
    }

    public async Task<Dictionary<Guid, GoogleResponse>> SendNotificationsAsync(
        GoogleNotificationRequest[] googleNotificationRequests)
    {
        var batch = new BatchRequest(googleService);
        var googleResponses = new Dictionary<Guid, GoogleResponse>();

        foreach (var googleNotificationRequest in googleNotificationRequests)
        {
            batch.Queue<UrlNotification>(googleService.UrlNotifications.Publish(googleNotificationRequest.Notification),
            (content, error, i, message) =>
            {
                var googleResponse = GetGoogleResponse(message, error);
                googleResponses.Add(googleNotificationRequest.TaskId, googleResponse);
            });
        }

        await batch.ExecuteAsync();
        return googleResponses;
    }

    private GoogleResponse GetGoogleResponse(HttpResponseMessage message, RequestError error)
    {
        var googleResponse = new GoogleResponse()
        {
            Time = DateTime.Now,
            StatusCode = (int)message.StatusCode,
            Message = message.ReasonPhrase
        };

        if (!message.IsSuccessStatusCode)
            googleResponse.ErrorReason = error.Message;

        return googleResponse;
    }
}
