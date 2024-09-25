public class GoogleTask
{
    public Guid Id { get; set; }
    public Uri Url { get; set; }
    public DateTime AddTime { get; set; }
    public bool IsPriority { get; set; }
    public Action UrlAction { get; set; }
    public bool IsCompleted { get; set; }
    public GoogleResponse GoogleResponse { get; set; }

    public enum Action
    {
        Update,
        Delete
    }
}

