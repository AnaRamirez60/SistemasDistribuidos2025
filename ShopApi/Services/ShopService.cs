using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ShopApi.Repositories;
using ShopApi.Mappers;
using ShopApi; 
using Microsoft.AspNetCore.Authorization.Infrastructure;
using ShopApi.Models;

namespace ShopApi.Services;
public class ShopService : ShopApi.ShopService.ShopServiceBase
{
    private readonly IShopRepository _shopRepository;
    private const int MinNameLength = 3;

    public ShopService(IShopRepository shopRepository)
    {
        _shopRepository = shopRepository;
    }

    public override async Task<CreateShopResponse> CreateShops(IAsyncStreamReader<CreateShopRequest> requestStream, ServerCallContext context)
    {
        var createdShops = new List<ShopResponse>();

        while (await requestStream.MoveNext(context.CancellationToken))
        {
            var request = requestStream.Current;
            var shop = request.ToModel();
            var shopExists = await _shopRepository.GetByNameAsync(shop.Name, context.CancellationToken);

            if (shopExists.Any())
                continue;
            var createdShop = await _shopRepository.CreateAsync(shop, context.CancellationToken);
            createdShops.Add(createdShop.ToResponse());
        }

        return new CreateShopResponse
        {
            SuccessCount = createdShops.Count,
            Shops = { createdShops }
        };
    }


    public override async Task GetAllShopsByName(ShopsByNameRequest request, IServerStreamWriter<ShopResponse> responseStream, ServerCallContext context)
    {
        var shops = await _shopRepository.GetByNameAsync(request.Name, context.CancellationToken);

        foreach(var shop in shops){
            if (context.CancellationToken.IsCancellationRequested)
                break;
            await responseStream.WriteAsync(shop.ToResponse());
            await Task.Delay(TimeSpan.FromSeconds(5), context.CancellationToken);
        }
    }
    public override async Task<ShopResponse> GetShopById(ShopByIdRequest request, ServerCallContext context)
    {
        var shop = await GetShopAsync(request.Id, context.CancellationToken);
        return shop.ToResponse();
    }

    public override async Task<Google.Protobuf.WellKnownTypes.Empty> DeleteShop(ShopByIdRequest request, ServerCallContext context)
    {
        if (!IdFormatIsValid(request.Id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "The provided ID format is invalid."));

        await GetShopAsync(request.Id, context.CancellationToken); 
        await _shopRepository.DeleteAsync(request.Id, context.CancellationToken);
        return new Google.Protobuf.WellKnownTypes.Empty();
    }

    public override async Task<Google.Protobuf.WellKnownTypes.Empty> UpdateShop(UpdateShopRequest request, ServerCallContext context)
    {
        var requestShop = new Shop
        {
            Id = request.Id,
            Name = request.Name,
            Category = request.Category,
            Description = request.Description,
            Price = request.Price,
            Cost = request.Cost,
            ExpirationDate = request.ExpirationDate == null ? DateTime.MinValue : request.ExpirationDate.ToDateTime(),
            Stock = request.Stock
        };
        if (!IdFormatIsValid(requestShop.Id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid item ID."));

        if (requestShop.ExpirationDate < DateTime.UtcNow)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid shop expiration date"));

        if (string.IsNullOrEmpty(requestShop.Name) || requestShop.Name.Length < MinNameLength)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid item name"));

        var existingShop = await GetShopAsync(requestShop.Id, context.CancellationToken);

        existingShop.Name = requestShop.Name;
        existingShop.Category = requestShop.Category;
        existingShop.Description = requestShop.Description;
        existingShop.Price = requestShop.Price;
        existingShop.Cost = requestShop.Cost;
        existingShop.ExpirationDate = requestShop.ExpirationDate;
        existingShop.Stock = requestShop.Stock;

        var shopExists = await _shopRepository.GetByNameAsync(existingShop.Name, context.CancellationToken);
        var shopAlreadyExist = shopExists.Any(s => s.Id != existingShop.Id);
        if (shopAlreadyExist)
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"Item already exists."));

        await _shopRepository.UpdateAsync(existingShop, context.CancellationToken);
        return new Google.Protobuf.WellKnownTypes.Empty();
    }

    private async Task<Shop> GetShopAsync (string id, CancellationToken cancellationToken)
    {
        var shop = await _shopRepository.GetByIdAsync(id, cancellationToken);
        return shop ?? throw new RpcException(new Status(StatusCode.NotFound, $"Item with ID {id} not found."));
    }

    private static bool IdFormatIsValid(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && id.Length > 20;
    }    

}
