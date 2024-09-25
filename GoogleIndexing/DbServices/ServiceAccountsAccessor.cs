using MongoDB.Driver;

public class ServiceAccountsAccessor
{
    private readonly IMongoCollection<ServiceAccount> collection;

    public ServiceAccountsAccessor(IMongoDatabase mongoDatabase)
    {
        collection = mongoDatabase.GetCollection<ServiceAccount>("ServiceAccounts");
    }

    public async Task<ServiceAccount> GetAccountAsync() =>
        await collection.Find(serviceAccount => serviceAccount.QuotaCount > 0).FirstOrDefaultAsync();

    public async Task ReduceQuotaAsync(ServiceAccount serviceAccount, int quotaSpent)
    {
        var filter = Builders<ServiceAccount>.Filter.Eq("Email", serviceAccount.Email);
        var updateInfoSetting = Builders<ServiceAccount>.Update.Set("QuotaCount", serviceAccount.QuotaCount - quotaSpent);

        await collection.UpdateOneAsync(filter, updateInfoSetting);
    }

    public async Task UpdateQuotaAsync()
    {
        var filter = Builders<ServiceAccount>.Filter.Empty;
        var updateInfoSetting = Builders<ServiceAccount>.Update.Set("QuotaCount", 200);

        await collection.UpdateManyAsync(filter, updateInfoSetting);
    }
}