using System.ServiceModel;
using peliculaApi.Infrastructure.Contracts;
using peliculaApi.Mappers;
using peliculaApi.Models;
using peliculaApi.Exceptions;
using System.Text.Json;
using peliculaApi.Infrastructure.Dtos;
using peliculaApi.Dtos;

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

    public async Task<PagedResponse<Pelicula>> GetPeliculasAsync(string title, string genre, int pageSize, int pageNumber, string orderBy, string orderDirection, CancellationToken cancellationToken)
    {
        var query = new Query
        {
            Title = title ?? string.Empty,
            Genre = genre ?? string.Empty,
            PageSize = pageSize,
            PageNumber = pageNumber,
            OrderBy = orderBy,
            OrderDirection = orderDirection
        };

        var paginated = await _peliculaContract.GetPeliculas(query, cancellationToken);
        
        // Convertir directamente sin aplicar filtros adicionales
        // ya que el servicio SOAP debería manejar el filtrado, paginación y ordenamiento
        var pagedResponse = paginated.ToPagedResponse();

        return new PagedResponse<Pelicula>
        {
            PageNumber = pagedResponse.PageNumber,
            PageSize = pagedResponse.PageSize,
            TotalRecords = pagedResponse.TotalRecords,
            TotalPages = pagedResponse.TotalPages,
            Data = pagedResponse.Data.ToList()
        };
    }

    public async Task UpdatePeliculaAsync(Pelicula pelicula, CancellationToken cancellationToken)
    {
        try
        {
            await _peliculaContract.UpdatePelicula(pelicula.ToUpdateRequest(), cancellationToken);
        }
        catch (FaultException ex) when (ex.Message == "pelicula not found")
        {
            throw new PeliculaNotFoundException(pelicula.Id);
        }
        catch (FaultException ex) when (ex.Message == "Another pelicula with the same title already exists")
        {
            throw new PeliculaAlreadyExistsException(pelicula.Title);
        }
    }

    public async Task DeletePeliculaAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _peliculaContract.DeletePelicula(id, cancellationToken);
        }
        catch (FaultException ex) when (ex.Message == "Pelicula not found")
        {
            _logger.LogWarning(ex, "Pelicula not found");
            throw new PeliculaNotFoundException(id);
        }
    }
    public async Task<IList<Pelicula>> GetPeliculasByTitleAsync(string title, CancellationToken cancellationToken)
    {
        _logger.LogDebug(":(");
        var peliculas = await _peliculaContract.GetPeliculasByTitle(title, cancellationToken);
        return peliculas.ToModel();
    }

     public async Task<Pelicula> CreatePeliculaAsync(Pelicula pelicula, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Sending request to soap api, with pelicula: {name}", pelicula.Title);
            var createdPelicula = await _peliculaContract.CreatePelicula(pelicula.ToRequest(), cancellationToken);
            return createdPelicula.ToModel();
        }
        catch (Exception e)
        {
            _logger.LogError(e, message: "Algo trono en el create pelicula a soap");
            throw;
        }
    }

}