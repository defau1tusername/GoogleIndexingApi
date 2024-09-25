using Microsoft.AspNetCore.Mvc;

public class ApiController : ControllerBase
{
    private readonly GoogleTasksAccessor googleTasksAccessor;
    private readonly TaskArchiveAccessor taskArchiveAccessor;

    public ApiController(GoogleTasksAccessor googleTasksAccessor,
        TaskArchiveAccessor taskArchiveAccessor)
    {
        this.googleTasksAccessor = googleTasksAccessor;
        this.taskArchiveAccessor = taskArchiveAccessor;
    }

    [Route("/")]
    [HttpGet]
    public IActionResult GetMainPage()
    {
        return Content(System.IO.File.ReadAllText("./html/main-page.html"), "text/html");
    }

    [Route("/api/tasks")]
    [HttpGet]
    public async Task<IActionResult> GetTasksAsync()
    {
        var tasks = await googleTasksAccessor.GetAllTasksAsync();

        return Ok(tasks);
    }

    [Route("/api/add-task")]
    [HttpPost]
    public async Task<IActionResult> AddTaskAsync([FromBody] AddTaskForm addTaskForm)
    {
        var tasks = new List<GoogleTask>();

        foreach (var url in addTaskForm.Urls)
        {
            var googleTask = new GoogleTask
            {
                Id = Guid.NewGuid(),
                Url = url,
                IsPriority = addTaskForm.IsPriority,
                UrlAction = addTaskForm.UrlAction,
                IsCompleted = false,
                AddTime = DateTime.Now
            };

            tasks.Add(googleTask);
        }

        await googleTasksAccessor.AddTaskAsync(tasks);

        return Ok(tasks);
    }

    [Route("/api/delete-task/{guid:Guid}")]
    [HttpDelete]
    public async Task<IActionResult> DeleteAdAsync([FromRoute] Guid guid)
    {
        var task = await googleTasksAccessor.GetTaskByIdAsync(guid);
        await googleTasksAccessor.DeleteTaskAsync(guid);
        await taskArchiveAccessor.AddTaskAsync(task);
        
        return Ok();
    }
}