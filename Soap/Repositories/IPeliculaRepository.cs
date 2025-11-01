using PeliculaApi.Models;
using PeliculaApi.Services;
using PeliculaApi.Dtos;

namespace PeliculaApi.Repositories;

public interface IPeliculaRepository
{
    Task<Pelicula> CreateAsync(Pelicula pelicula, CancellationToken cancellationToken);
    Task<Pelicula> GetPeliculaByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Pelicula>> GetPeliculasByTitleAsync(string title, CancellationToken cancellationToken);
    Task<IReadOnlyList<Pelicula>> GetPeliculasByGenreAsync(string genre, CancellationToken cancellationToken);
    Task DeletePeliculaAsync(Pelicula pelicula, CancellationToken cancellationToken);
    Task UpdatePeliculaAsync(Pelicula pelicula, CancellationToken cancellationToken);
    Task<PagedResponseDto> GetPeliculasAsync(Query query, CancellationToken cancellationToken);


}
