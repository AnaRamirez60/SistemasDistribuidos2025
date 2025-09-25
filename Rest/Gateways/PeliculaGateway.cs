using System.ServiceModel;
using peliculaApi.Infrastructure.Contracts;
using peliculaApi.Mappers;
using peliculaApi.Models;
using peliculaApi.Exceptions;
using System.Text.Json;
using peliculaApi.Infrastructure.Dtos;

namespace peliculaApi.Gateways;

public class PeliculaGateway : IPeliculaGateway
{
    private readonly IPeliculaContract _peliculaContract;
    private readonly ILogger<PeliculaGateway> _logger;

    public PeliculaGateway(IConfiguration configuration, ILogger<PeliculaGateway> logger)
    {
        var binding = new BasicHttpBinding();
        var endpoint = new EndpointAddress(uri: configuration.GetValue<string>(key: "PeliculaService:Url"));
        _peliculaContract = new ChannelFactory<IPeliculaContract>(binding, endpoint).CreateChannel();
        _logger = logger;
    }

   public async Task<Pelicula> GetPeliculaByIdAsync(Guid id, CancellationToken cancellationToken)
{
    try
    {
        var peliculaDto = await _peliculaContract.GetPeliculaById(id, cancellationToken);

if (peliculaDto == null)
{
    return null;
}

var peliculaJson = JsonSerializer.Serialize(peliculaDto);

return peliculaDto.ToModel();
    }
    catch (FaultException ex) when (ex.Message.Contains("Pelicula not found"))
    {
        _logger.LogWarning(ex, "Pelicula not found");
        return null;
    }
}

}