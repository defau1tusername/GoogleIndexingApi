
using MongoDB.Driver;
using SharpCompress.Archives.Tar;

public class TaskArchiveAccessor
{
    private readonly IMongoCollection<GoogleTask> collection;

    public TaskArchiveAccessor(IMongoDatabase mongoDatabase)
    {
        collection = mongoDatabase.GetCollection<GoogleTask>("TaskArchive");
    }

    public async Task AddTaskAsync(GoogleTask task) =>
        await collection.InsertOneAsync(task);
}