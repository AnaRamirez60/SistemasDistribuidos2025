namespace ShopApi.Infrastructure;

public class MongoDBSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string ShopsCollectionName { get; set; } = null!;
}