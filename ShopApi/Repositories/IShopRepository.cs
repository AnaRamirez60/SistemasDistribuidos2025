using ShopApi.Models;

namespace ShopApi.Repositories;

public interface IShopRepository
{
    Task<Shop> CreateAsync(Shop shop, CancellationToken cancellationToken);

    Task<IEnumerable<Shop>> GetByNameAsync(string name, CancellationToken cancellationToken);
    }