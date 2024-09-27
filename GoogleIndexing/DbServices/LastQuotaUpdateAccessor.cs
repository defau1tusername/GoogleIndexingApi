
using MongoDB.Driver;

public class LastQuotaUpdateAccessor
{
    private readonly IMongoCollection<LastQuotaUpdate> collection;

    public LastQuotaUpdateAccessor(IMongoDatabase mongoDatabase)
    {
        collection = mongoDatabase.GetCollection<LastQuotaUpdate>("LastQuotaUpdate");
    }

    public async Task<DateTime> GetValueAsync()
    {
        var lastQuotaUpdate = await collection.Find(_ => true).FirstOrDefaultAsync();
        return lastQuotaUpdate.Value;
    }

    public async Task SetValueAsync(DateTime date)
    {
        var filter = Builders<LastQuotaUpdate>.Filter.Empty;
        var updateInfoSetting = Builders<LastQuotaUpdate>.Update.Set("Value", date);

        await collection.UpdateOneAsync(filter, updateInfoSetting);
    }
}