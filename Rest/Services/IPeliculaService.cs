using peliculaApi.Dtos;
using peliculaApi.Models;

namespace peliculaApi.Services
{
    public interface IPeliculaService
    {
        Task<Pelicula> GetPeliculaByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<PagedResponse<Pelicula>> GetPeliculasAsync(string title, string genre, int pageSize, int pageNumber, string orderBy, string orderDirection, CancellationToken cancellationToken);
        Task<Pelicula> CreatePeliculaAsync(Pelicula pelicula, CancellationToken cancellationToken);
        Task DeletePeliculaAsync(Guid id, CancellationToken cancellationToken);
        Task UpdatePeliculaAsync(Pelicula pelicula, CancellationToken cancellationToken);

        Task<Pelicula> PatchPeliculaAsync(Guid id, string? title, string? director, int? releaseYear, string? genre, int? duration, CancellationToken cancellationToken);
    }
}