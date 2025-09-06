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

    public async Task<PeliculaResponseDto> GetPeliculaByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var pelicula = await _peliculaRepository.GetPeliculaByIdAsync(id, cancellationToken);
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