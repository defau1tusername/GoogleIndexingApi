public class AddTaskForm
{
    public Uri[] Urls { get; set; }
    public bool IsPriority { get; set; }
    public GoogleTask.Action UrlAction { get; set; }
}