using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using ShopApi.Infrastructure;
using ShopApi.Infrastructure.Documents;
using ShopApi.Mappers;
using ShopApi.Models;

namespace ShopApi.Repositories;

public class ShopRepository : IShopRepository
{
    private readonly IMongoCollection<ShopDocument> _shopsCollection;

    public ShopRepository(IMongoDatabase database, IOptions<MongoDBSettings> settings)
    {
        _shopsCollection = database.GetCollection<ShopDocument>(settings.Value.ShopsCollectionName);
    }

    public async Task<Shop> CreateAsync(Shop shop, CancellationToken cancellationToken)
    {
        var shopToCreate = shop.ToDocument();
        await _shopsCollection.InsertOneAsync(shopToCreate, cancellationToken: cancellationToken);
        return shopToCreate.ToDomain();
    }

    public async Task<IEnumerable<Shop>> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var shops = await _shopsCollection.Find(filter: t => t.Name.Contains(name)).ToListAsync(cancellationToken);
        return shops.Select(selector: t => t.ToDomain());
    }

     public async Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        await _shopsCollection.DeleteOneAsync(t => t.Id == id, cancellationToken);

    }  

    public async Task UpdateAsync(Shop shop, CancellationToken cancellationToken)
    {
        var update = Builders<ShopDocument>.Update
            .Set(t => t.Name, shop.Name)
            .Set(t => t.Category, shop.Category)
            .Set(t => t.Description, shop.Description)
            .Set(t => t.Price, shop.Price)
            .Set(t => t.Cost, shop.Cost)
            .Set(t => t.ExpirationDate, shop.ExpirationDate)
            .Set(t => t.Stock, shop.Stock);

        await _shopsCollection.UpdateOneAsync(t => t.Id == shop.Id, update, cancellationToken: cancellationToken);
    } 

    public async Task<Shop?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var shop = await _shopsCollection.Find(t => t.Id == id).FirstOrDefaultAsync(cancellationToken);
        return shop.ToDomain();
    }
}