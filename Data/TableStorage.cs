using Azure;
using Azure.Data.Tables;
using System.Linq;

namespace easyurl.Data;

public class TableStorage : ITableStorage
{
    private readonly TableClient tableClient;
    private string tableName = "ShortenedUrls";

    public TableStorage()
    {
        tableClient = new TableClient(Environment.GetEnvironmentVariable("AzureTableStorageConnectionString"), tableName);
        tableClient.CreateIfNotExistsAsync();
    }
    public async Task SaveShortenedUrl(string originalUrl, string baseShortenedUrl, string fullShortenedUrl, string calledIP)
    {
        if (string.IsNullOrEmpty(originalUrl) || string.IsNullOrEmpty(fullShortenedUrl))
        {
            throw new ArgumentNullException("Missing required parameters");
        }

        var tableEntity = new TableEntity("urls", baseShortenedUrl)
        {
            {"OriginalUrl",  originalUrl},
            {"ShortenedUrl" , fullShortenedUrl},
            {"CalledIP", calledIP},
            {"Timestamp", DateTime.UtcNow}
        };

        await tableClient.AddEntityAsync(tableEntity);
    }

    public async Task<bool> CheckIfKeyExists(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException("Missing required parameters");
        }

        var response = await tableClient.GetEntityIfExistsAsync<TableEntity>("urls", key);
        return response.HasValue;
    }

    public async Task<TableEntity []> GetAllEntities()
    {

        IList<TableEntity> entities = new List<TableEntity>();
        var ents = tableClient.QueryAsync<TableEntity>();
        await foreach (var ent in ents)
        {
            entities.Add(ent);
        }
        return entities.ToArray();
    }

    public string GetShortenedUrl(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException("Missing required parameters");
        }

        var response = tableClient.GetEntityIfExists<TableEntity>("urls", key);
        response.Value.TryGetValue("OriginalUrl", out var originalUrl);
        if (originalUrl == null)
        {
            originalUrl = "";
        }
        return originalUrl.ToString();
    }

    public async Task DeleteEntity(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException("Missing required parameters");
        }

        await tableClient.DeleteEntityAsync("urls", key);
    }
}

public interface ITableStorage
{
    Task SaveShortenedUrl(string originalUrl, string baseShortenedUrl, string fullShortenedUrl, string calledIP);
    string GetShortenedUrl(string key);
    Task<bool> CheckIfKeyExists(string key);
    Task<TableEntity []> GetAllEntities();
    Task DeleteEntity(string key);
}