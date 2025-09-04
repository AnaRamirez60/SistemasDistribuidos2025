using PeliculaApi.Models;
using PeliculaApi.Infrastructure.Entities;
using PeliculaApi.Dtos;
using Microsoft.EntityFrameworkCore;

namespace PeliculaApi.Mappers;

public static class PeliculaMapper
{
    //Extension method
    public static Pelicula ToModel(this PeliculaEntity peliculaEntity)
    {
        if (peliculaEntity is null)
        {
            return null;
        }

        return new Pelicula
        {
            Id = peliculaEntity.Id,
            Title = peliculaEntity.Title,
            Director = peliculaEntity.Director,
            ReleaseYear = peliculaEntity.ReleaseYear,
            Genre = peliculaEntity.Genre,
            Duration = peliculaEntity.Duration
        };
    }

    public static PeliculaEntity ToEntity(this Pelicula pelicula)
    {
        return new PeliculaEntity
        {
            Id = pelicula.Id,
            Title = pelicula.Title,
            Director = pelicula.Director,
            ReleaseYear = pelicula.ReleaseYear,
            Genre = pelicula.Genre,
            Duration = pelicula.Duration
        };
    }

    public static Pelicula ToModel(this CreatePeliculaDto requestPeliculaDto)
    {
        return new Pelicula
        {
            Title = requestPeliculaDto.Title,
            Director = requestPeliculaDto.Director,
            ReleaseYear = requestPeliculaDto.ReleaseYear,
            Genre = requestPeliculaDto.Genre,
            Duration = requestPeliculaDto.Duration
        };
    }

    public static PeliculaResponseDto ToResponseDto(this Pelicula pelicula)
    {
        return new PeliculaResponseDto
        {
            Id = pelicula.Id,
            Title = pelicula.Title,
            Director = pelicula.Director,
            ReleaseYear = pelicula.ReleaseYear,
            Genre = pelicula.Genre,
            Duration = pelicula.Duration
        };
    }

    public static IList<PeliculaResponseDto> ToResponseDto(this IReadOnlyList<Pelicula> peliculas)
    {
        return peliculas.Select(s => s.ToResponseDto()).ToList();
    }

    public static IReadOnlyList<Pelicula> ToModel(this IReadOnlyList<PeliculaEntity> peliculas)
    {
        return peliculas.Select(s => s.ToModel()).ToList();
    }

}