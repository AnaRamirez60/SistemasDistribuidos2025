using peliculaApi.Models;
using peliculaApi.Gateways;
using peliculaApi.Exceptions;

namespace peliculaApi.Services;

public class PeliculaService : IPeliculaService
{
    private readonly IPeliculaGateway _peliculaGateway;

    public PeliculaService(IPeliculaGateway peliculaGateway)
    {
        _peliculaGateway = peliculaGateway;
    }

    public async Task<Pelicula> GetPeliculaByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            throw new InvalidIdException(id);
        }

        var pelicula = await _peliculaGateway.GetPeliculaByIdAsync(id, cancellationToken);

        if (pelicula is null)
        {
            throw new PeliculaNotFoundException(id);
        }

        return pelicula;
    }

}