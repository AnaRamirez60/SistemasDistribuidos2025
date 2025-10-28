using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ShopApi.Repositories;
using ShopApi.Mappers;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace ShopApi.Services;
public class ShopService : ShopApi.ShopService.ShopServiceBase
{
    private readonly IShopRepository _shopRepository;

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
}