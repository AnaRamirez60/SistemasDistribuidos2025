using peliculaApi.Infrastructure.Dtos;
using peliculaApi.Models;
using peliculaApi.Dtos;

namespace peliculaApi.Mappers;

public static class PeliculaMapper
{
    public static Pelicula ToModel(this PeliculaResponseDto peliculaResponseDto)
    {
        return new Pelicula
        {
            Id = peliculaResponseDto.Id,
            Title = peliculaResponseDto.Title,
            Director = peliculaResponseDto.Director,
            ReleaseYear = peliculaResponseDto.ReleaseYear,
            Genre = peliculaResponseDto.Genre,
            Duration = peliculaResponseDto.Duration
        };
    }

    public static IList<Pelicula> ToModel(this IList<PeliculaResponseDto> peliculaResponseDtos)
    {
        return peliculaResponseDtos.Select(s => s.ToModel()).ToList();
    }
    public static PeliculaResponseDto ToResponse(this Pelicula pelicula)
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

    public static IList<PeliculaResponseDto> ToResponse(this IList<Pelicula> peliculas)
    {
        return peliculas.Select(s => s.ToResponse()).ToList();
    }

}