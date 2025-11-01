using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ShopApi.Infrastructure.Documents;

public class ShopDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("category")]
    public string Category { get; set; }

    [BsonElement("description")]
    public string Description { get; set; } 

    [BsonElement("price")]
    public int Price { get; set; }

    [BsonElement("cost")]
    public int Cost { get; set; }

    [BsonElement("expiration_date")]
    public DateTime ExpirationDate { get; set; }

    [BsonElement("stock")]
    public int Stock { get; set; }
}
