using peliculaApi.Models;
using peliculaApi.Infrastructure.Dtos;
using peliculaApi.Dtos;

namespace peliculaApi.Gateways;

public interface IPeliculaGateway
{
    Task<Pelicula> GetPeliculaByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IList<Pelicula>> GetPeliculasByTitleAsync(string title, CancellationToken cancellationToken);

    Task<Pelicula> CreatePeliculaAsync(Pelicula pelicula, CancellationToken cancellationToken);

    Task DeletePeliculaAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResponse<Pelicula>> GetPeliculasAsync(string title, string genre, int pageSize, int pageNumber, string orderBy, string orderDirection, CancellationToken cancellationToken);

    Task UpdatePeliculaAsync(Pelicula pelicula, CancellationToken cancellationToken);

}