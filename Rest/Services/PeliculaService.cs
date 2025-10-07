using peliculaApi.Models;
using peliculaApi.Gateways;
using peliculaApi.Exceptions;
using peliculaApi.Dtos;

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

    public async Task<PagedResponse<Pelicula>> GetPeliculasAsync(string title, string genre, int pageSize, int pageNumber, string orderBy, string orderDirection, CancellationToken cancellationToken)
    {
        return await _peliculaGateway.GetPeliculasAsync(title, genre, pageSize, pageNumber, orderBy, orderDirection, cancellationToken);
    }

    public async Task<Pelicula> CreatePeliculaAsync(Pelicula pelicula, CancellationToken cancellationToken)
    {
        return await _peliculaGateway.CreatePeliculaAsync(pelicula, cancellationToken);
    }

    public async Task DeletePeliculaAsync(Guid id, CancellationToken cancellationToken)
    {
        await _peliculaGateway.DeletePeliculaAsync(id, cancellationToken);
    }

    public async Task UpdatePeliculaAsync(Pelicula pelicula, CancellationToken cancellationToken)
    {
        await _peliculaGateway.UpdatePeliculaAsync(pelicula, cancellationToken);
    }

    public async Task<Pelicula> PatchPeliculaAsync(Guid id, string? title, string? director, int? releaseYear, string? genre, int? duration, CancellationToken cancellationToken)
    {
        var pelicula = await _peliculaGateway.GetPeliculaByIdAsync(id, cancellationToken);
        if (pelicula == null)
        {
            throw new PeliculaNotFoundException(id);
        }

        pelicula.Title = title ?? pelicula.Title;
        pelicula.Director = director ?? pelicula.Director;
        pelicula.ReleaseYear = releaseYear ?? pelicula.ReleaseYear;
        pelicula.Genre = genre ?? pelicula.Genre;
        pelicula.Duration = duration ?? pelicula.Duration;

        await _peliculaGateway.UpdatePeliculaAsync(pelicula, cancellationToken);
        return pelicula;
    }

}