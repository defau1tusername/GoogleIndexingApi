using MongoDB.Driver;

public class GoogleTasksAccessor
{
    private readonly IMongoCollection<GoogleTask> collection;

    public GoogleTasksAccessor(IMongoDatabase mongoDatabase)
    {
        collection = mongoDatabase.GetCollection<GoogleTask>("GoogleTasks");
    }

    public async Task AddTaskAsync(List<GoogleTask> googleTasks) =>
        await collection.InsertManyAsync(googleTasks);

    public async Task<List<GoogleTask>> GetTasksAsync(int skip, int limit) =>
        await collection.Find(_ => true).SortByDescending(task => task.AddTime).Skip(skip).Limit(limit).ToListAsync();

    public Task<List<GoogleTask>> GetTasksForUpdateAsync(int quotaCount, bool highPriorityOnly = false) =>
        highPriorityOnly
            ? collection.Find(task => !task.IsCompleted && task.IsPriority).Limit(quotaCount).ToListAsync()
            : collection.Find(task => !task.IsCompleted).Limit(quotaCount).ToListAsync();

    public async Task UpdateGoogleResponsesAsync(Dictionary<Guid, GoogleResponse> googleResponses)
    {
        foreach (var googleResponse in googleResponses)
        {
            var filter = Builders<GoogleTask>.Filter.Eq("Id", googleResponse.Key);

            var updateInfoSetting = Builders<GoogleTask>.Update.Combine(
                Builders<GoogleTask>.Update.Set("GoogleResponse", googleResponse.Value),
                Builders<GoogleTask>.Update.Set("IsCompleted", true)
            );

            await collection.UpdateOneAsync(filter, updateInfoSetting);
        }
    }

    public async Task DeleteTaskAsync(Guid guid) =>
        await collection.DeleteOneAsync(task => task.Id == guid);

    public async Task<GoogleTask> GetTaskByIdAsync(Guid guid) =>
        await collection.Find(task => task.Id == guid).FirstOrDefaultAsync();
}
