using PeliculaApi.Dtos;
using PeliculaApi.Models;
using PeliculaApi.Repositories;
using System.ServiceModel;
using PeliculaApi.Mappers;
using PeliculaApi.Validators;
using PeliculaApi.Services;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace PeliculaApi.Services;

public class PeliculaService : IPeliculaService
{
    private readonly IPeliculaRepository _peliculaRepository;

    public PeliculaService(IPeliculaRepository peliculaRepository)
    {
        _peliculaRepository = peliculaRepository;
    }

   public async Task<PeliculaResponseDto> UpdatePelicula(UpdatePeliculaDto peliculaToUpdate, CancellationToken cancellationToken)
    {
        var pelicula = await _peliculaRepository.GetPeliculaByIdAsync(peliculaToUpdate.Id, cancellationToken);
        if (!PeliculaExists(pelicula))
        {
            throw new FaultException(reason: "Pelicula not found");
        }

        if(!await IsPeliculaAllowedToBeUpdated(peliculaToUpdate, cancellationToken))
        {
            throw new FaultException("Another pelicula with the same name already exists");
        }

        pelicula.Title = peliculaToUpdate.Title;
        pelicula.Director = peliculaToUpdate.Director;
        pelicula.ReleaseYear = peliculaToUpdate.ReleaseYear;
        pelicula.Genre = peliculaToUpdate.Genre;
        pelicula.Duration = peliculaToUpdate.Duration;

        await _peliculaRepository.UpdatePeliculaAsync(pelicula, cancellationToken);
        return pelicula.ToResponseDto();
    }

    private async Task<bool> IsPeliculaAllowedToBeUpdated(UpdatePeliculaDto peliculaToUpdate, CancellationToken cancellationToken)
    {
        var duplicatedPeliculas = await _peliculaRepository.GetPeliculasByTitleAsync(peliculaToUpdate.Title, cancellationToken);
        return !duplicatedPeliculas.Any(p => p.Id != peliculaToUpdate.Id);
    }

    private bool IsTheSamePelicula(Pelicula pelicula, UpdatePeliculaDto peliculaToUpdate)
    {
        return pelicula.Id != peliculaToUpdate.Id;
    }

    public async Task<IList<PeliculaResponseDto>> GetPeliculasByTitleAsync(string title, CancellationToken cancellationToken)
    {
        var peliculas = await _peliculaRepository.GetPeliculasByTitleAsync(title, cancellationToken);
        return peliculas.ToResponseDto();
    }

    public async Task<IList<PeliculaResponseDto>> GetPeliculasByGenreAsync(string genre, CancellationToken cancellationToken)
    {
        var peliculas = await _peliculaRepository.GetPeliculasByGenreAsync(genre, cancellationToken);
        return peliculas.ToResponseDto();
    }

    public async Task<PagedResponseDto> GetPeliculas(Query query, CancellationToken cancellationToken)
    {
        // Debug: Log de parámetros recibidos en el servicio
        Console.WriteLine($"Servicio SOAP - Query recibida: Title='{query.Title}', Type='{query.Type}', PageNumber={query.PageNumber}, PageSize={query.PageSize}, OrderBy='{query.OrderBy}', OrderDirection='{query.OrderDirection}'");
        
        // Validar parámetros de paginación
        if (query.PageNumber < 1)
        {
            Console.WriteLine($"Error: PageNumber inválido: {query.PageNumber}");
            throw new FaultException("PageNumber must be greater than 0");
        }
        
        if (query.PageSize <= 0 || query.PageSize > 100)
        {
            Console.WriteLine($"Error: PageSize inválido: {query.PageSize}");
            throw new FaultException("PageSize must be between 1 and 100");
        }

        Console.WriteLine("Parámetros válidos, enviando al repository...");
        var result = await _peliculaRepository.GetPeliculasAsync(query, cancellationToken);
        Console.WriteLine($"Resultado: {result.Data.Count} registros de {result.TotalRecords} totales, página {result.PageNumber}/{result.TotalPages}");
        
        return result;
    }

    public async Task<DeletePeliculaResponseDto> DeletePelicula(Guid id, CancellationToken cancellationToken)
    {
        var pelicula = await _peliculaRepository.GetPeliculaByIdAsync(id, cancellationToken);
        if (!PeliculaExists(pelicula))
        {
            throw new FaultException("Pelicula not found");
        }

        await _peliculaRepository.DeletePeliculaAsync(pelicula, cancellationToken);
        return new DeletePeliculaResponseDto { Success = true };
    }

    public async Task<PeliculaResponseDto> GetPeliculaByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Console.WriteLine($"=== GetPeliculaByIdAsync DEBUG ===");
        Console.WriteLine($"ID recibido: {id}");
        Console.WriteLine($"ID tipo: {id.GetType()}");
        Console.WriteLine($"ID como string: '{id.ToString()}'");
        
        var pelicula = await _peliculaRepository.GetPeliculaByIdAsync(id, cancellationToken);
        Console.WriteLine($"Película obtenida del repository: {(pelicula == null ? "NULL" : $"ID={pelicula.Id}, Title='{pelicula.Title}'")}");
        Console.WriteLine($"PeliculaExists resultado: {PeliculaExists(pelicula)}");
        
        return PeliculaExists(pelicula) ? pelicula.ToResponseDto() : throw new FaultException("Pelicula not found");
    }

    public async Task<PeliculaResponseDto> CreatePelicula(CreatePeliculaDto peliculaRequest, CancellationToken cancellationToken)
    {
        //Fluent Methods
        peliculaRequest
            .ValidateTitle()
            .ValidateGenre();
        if (await IsPeliculaDuplicated(peliculaRequest.Title, cancellationToken))
        {
            throw new FaultException("Pelicula already exists");
        }

        var pelicula = await _peliculaRepository.CreateAsync(peliculaRequest.ToModel(), cancellationToken);

        return pelicula.ToResponseDto();
    }

    private static bool PeliculaExists(Pelicula? pelicula)
    {
        return pelicula is not null;
    }

     private async Task<bool> IsPeliculaDuplicated(string title, CancellationToken cancellationToken)
    {
        var peliculas = await _peliculaRepository.GetPeliculasByTitleAsync(title, cancellationToken);
        return peliculas.Any();
    }

}