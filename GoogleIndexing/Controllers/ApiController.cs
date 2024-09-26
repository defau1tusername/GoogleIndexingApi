using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

public class ApiController : ControllerBase
{
    private readonly GoogleTasksAccessor googleTasksAccessor;
    private readonly ServiceAccountsAccessor serviceAccountsAccessor;
    private readonly TaskArchiveAccessor taskArchiveAccessor;

    public ApiController(GoogleTasksAccessor googleTasksAccessor,
        ServiceAccountsAccessor serviceAccountsAccessor,
        TaskArchiveAccessor taskArchiveAccessor)
    {
        this.googleTasksAccessor = googleTasksAccessor;
        this.serviceAccountsAccessor = serviceAccountsAccessor;
        this.taskArchiveAccessor = taskArchiveAccessor;
    }

    [Route("/")]
    [HttpGet]
    public IActionResult GetMainPage()
    {
        return Content(System.IO.File.ReadAllText("./html/main-page.html"), "text/html");
    }

    [Route("/service-accounts")]
    [HttpGet]
    public IActionResult GetServiceAccountsPage()
    {
        return Content(System.IO.File.ReadAllText("./html/service-accounts.html"), "text/html");
    }

    [Route("/api/tasks")]
    [HttpGet]
    public async Task<IActionResult> GetTasksAsync([FromQuery] int skip, [FromQuery] int limit)
    {
        var tasks = await googleTasksAccessor.GetTasksAsync(skip, limit);

        return Ok(tasks);
    }   

    [Route("/api/quota")]
    [HttpGet]
    public async Task<IActionResult> GetQuotaLimitsAsync()
    {
        var serviceAccountsCount = await serviceAccountsAccessor.GetServiceAccountsCountAsync();
        var totalQuota = serviceAccountsCount * 200;
        var remainingQuota = await serviceAccountsAccessor.GetRemainingQuota();
        var quotaInfo = new QuotaInfo()
        {
            TotalQuota = totalQuota,
            RemainingQuota = remainingQuota
        };

        return Ok(quotaInfo);
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

    [Route("/api/service-accounts")]
    [HttpGet]
    public async Task<IActionResult> GetServiceAccountsAsync()
    {
        var serviceAccounts = await serviceAccountsAccessor.GetServiceAccountsAsync();
        var serviceAccountsView = new List<ServiceAccountDto>();
        foreach (var serviceAccount in serviceAccounts)
            serviceAccountsView.Add(new ServiceAccountDto()
            {
                ShortEmail = serviceAccount.Email[..9] + "***",
                RemainingQuota = serviceAccount.QuotaCount,
                TotalQuota = 200
            });

        return Ok(serviceAccountsView);
    }
}