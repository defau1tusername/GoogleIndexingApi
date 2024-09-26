public class QuotaUpdaterBgService : BackgroundService
{
    private readonly LastQuotaUpdateAccessor lastQuotaUpdateAccessor;
    private readonly ServiceAccountsAccessor serviceAccountsAccessor;

    public QuotaUpdaterBgService(LastQuotaUpdateAccessor lastQuotaUpdateAccessor,
        ServiceAccountsAccessor serviceAccountsAccessor)
    {
        this.lastQuotaUpdateAccessor = lastQuotaUpdateAccessor;
        this.serviceAccountsAccessor = serviceAccountsAccessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var dateLastUpdate = await lastQuotaUpdateAccessor.GetValueAsync();

        if (DateTime.Today > dateLastUpdate)
            await UpdateQuotaAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var nextUpdateTime = DateTime.Today.AddDays(1);
            var delay = nextUpdateTime - DateTime.Now;
            await Task.Delay(delay, stoppingToken);
            await UpdateQuotaAsync(stoppingToken);
        }
    }

    private async Task UpdateQuotaAsync(CancellationToken stoppingToken)
    {
        await serviceAccountsAccessor.UpdateQuotaAsync();
        await lastQuotaUpdateAccessor.SetValueAsync(DateTime.Today);
    }
}