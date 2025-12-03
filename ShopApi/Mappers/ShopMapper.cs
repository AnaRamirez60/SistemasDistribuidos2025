using ShopApi.Infrastructure.Documents;
using ShopApi.Models;
using Google.Protobuf.WellKnownTypes;
using MongoDB.Bson;

namespace ShopApi.Mappers;

public static class ShopMapper
{
    public static Shop ToDomain(this ShopDocument document)
    {
        if (document is null) throw new ArgumentNullException(nameof(document));
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
        if (shop is null) throw new ArgumentNullException(nameof(shop));
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
        if (request is null) throw new ArgumentNullException(nameof(request));
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
        if (shop is null) throw new ArgumentNullException(nameof(shop));
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