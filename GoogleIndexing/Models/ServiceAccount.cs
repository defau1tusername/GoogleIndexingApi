using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class ServiceAccount
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Email { get; set; }
    public string Key { get; set; }
    public int QuotaCount { get; set; }
}

