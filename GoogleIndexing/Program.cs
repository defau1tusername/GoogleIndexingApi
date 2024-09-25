using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;


var builder = WebApplication.CreateBuilder();
builder.Services.AddMvcCore()
    .AddControllersAsServices();

builder.Services.AddHostedService<TaskExecutorBgService>();
builder.Services.AddHostedService<QuotaUpdaterBgService>();

//DB SERVICE
BsonSerializer.RegisterSerializer(DateTimeSerializer.LocalInstance);
BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

builder.Services.Configure<GoogleIndexingApiDatabaseSettings>(builder.Configuration.GetSection("GoogleIndexingApiDatabase"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<GoogleIndexingApiDatabaseSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<GoogleIndexingApiDatabaseSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddSingleton<ServiceAccountsAccessor>();
builder.Services.AddSingleton<GoogleTasksAccessor>();
builder.Services.AddSingleton<LastQuotaUpdateAccessor>();
builder.Services.AddSingleton<TaskArchiveAccessor>();

//=========================================

builder.Services.AddMvc()
    .AddJsonOptions(opts =>
    {
        var enumConverter = new JsonStringEnumConverter();
        opts.JsonSerializerOptions.Converters.Add(enumConverter);
    });

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
        context.Context.Response.Headers.Add("Expires", "-1");
    }
});

app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());
app.Run();