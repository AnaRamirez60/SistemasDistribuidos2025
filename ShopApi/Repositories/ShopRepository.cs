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
 }