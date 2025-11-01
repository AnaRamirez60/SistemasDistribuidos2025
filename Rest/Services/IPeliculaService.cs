using peliculaApi.Dtos;
using peliculaApi.Models;

namespace peliculaApi.Services
{
    public interface IPeliculaService
    {
        Task<Pelicula> GetPeliculaByIdAsync(Guid id, CancellationToken cancellationToken);    }
}