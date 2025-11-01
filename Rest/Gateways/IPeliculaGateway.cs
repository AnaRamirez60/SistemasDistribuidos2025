using peliculaApi.Models;

namespace peliculaApi.Gateways;

public interface IPeliculaGateway
{
    Task<Pelicula> GetPeliculaByIdAsync(Guid id, CancellationToken cancellationToken);
}