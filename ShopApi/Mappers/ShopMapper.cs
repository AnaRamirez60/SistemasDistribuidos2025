using ShopApi.Infrastructure.Documents;
using ShopApi.Models;
using Google.Protobuf.WellKnownTypes;

namespace ShopApi.Mappers;

public static class ShopMapper
{
    public static Shop ToDomain(this ShopDocument document)
    {
        if (document is null)
        {
            return null;
        }
        return new Shop
        {
            Id = document.Id,
            Name = document.Name,
            Category = document.Category,
            Description = document.Description,
            Price = document.Price,
            Cost = document.Cost,
            ExpirationDate = document.ExpirationDate,
            Stock = document.Stock,
        };
    }

    public static ShopResponse ToResponse(this Shop shop)
    {
        return new ShopResponse
        {
            Id = shop.Id,
            Name = shop.Name,
            Category = shop.Category,
            Description = shop.Description,
            Price = shop.Price,
            Cost = shop.Cost,
            ExpirationDate = Timestamp.FromDateTime(shop.ExpirationDate.ToUniversalTime()),
            Stock = shop.Stock,
        };
    }

    public static Shop ToModel(this CreateShopRequest request)
    {
        return new Shop
        {
            Name = request.Name,
            Category = request.Category,
            Description = request.Description,
            Price = request.Price,
            Cost = request.Cost,
            ExpirationDate = request.ExpirationDate.ToDateTime(),
            Stock = request.Stock
        };
    }

    public static ShopDocument ToDocument(this Shop shop)
    {
        return new ShopDocument
        {
            Id = shop.Id,
            Name = shop.Name,
            Category = shop.Category,
            Description = shop.Description,
            Price = shop.Price,
            Cost = shop.Cost,
            ExpirationDate = shop.ExpirationDate,
            Stock = shop.Stock,
        };
    }
}