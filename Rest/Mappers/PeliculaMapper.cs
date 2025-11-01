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

    public static PeliculaResponse ToPeliculaResponse(this Pelicula pelicula)
    {
        return new PeliculaResponse
        {
            Id = pelicula.Id,
            Title = pelicula.Title,
            Director = pelicula.Director,
            ReleaseYear = pelicula.ReleaseYear,
            Genre = pelicula.Genre,
            Duration = pelicula.Duration
        };
    }

    public static Pelicula ToModel(this UpdatePeliculaRequest pelicula, Guid id)
    {
        return new Pelicula
        {
            Id = id,
            Title = pelicula.Title,
            Director = pelicula.Director,
            ReleaseYear = pelicula.ReleaseYear,
            Genre = pelicula.Genre,
            Duration = pelicula.Duration
        };
    }

    public static Pelicula ToModel(this CreatePeliculaRequest createPeliculaRequest)
    {
        return new Pelicula
        {
            Id = Guid.NewGuid(),
            Title = createPeliculaRequest.Title,
            Director = createPeliculaRequest.Director,
            ReleaseYear = createPeliculaRequest.ReleaseYear,
            Genre = createPeliculaRequest.Genre,
            Duration = createPeliculaRequest.Duration
        };
    }

    public static CreatePeliculaDto ToRequest(this Pelicula pelicula)
    {
        return new CreatePeliculaDto
        {
            Title = pelicula.Title,
            Director = pelicula.Director,
            ReleaseDate = new DateTime(pelicula.ReleaseYear, 1, 1),
            Genre = pelicula.Genre,
            Duration = pelicula.Duration
        };
    }

    public static UpdatePeliculaDto ToUpdateRequest(this Pelicula pelicula)
    {
        return new UpdatePeliculaDto
        {
            Id = pelicula.Id,
            Title = pelicula.Title,
            Director = pelicula.Director,
            ReleaseYear = pelicula.ReleaseYear,
            Genre = pelicula.Genre,
            Duration = pelicula.Duration
        };
    }


    public static IEnumerable<PeliculaResponse> ToResponse(this IEnumerable<Pelicula> peliculas)
    {
        return peliculas.Select(p => p.ToPeliculaResponse());
    }

    public static PagedResponse<PeliculaResponse> ToResponse(this PagedResult<Pelicula> pagedResult)
    {
        return new PagedResponse<PeliculaResponse>
        {
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalRecords = pagedResult.TotalRecords,
            TotalPages = pagedResult.TotalPages,
            Data = pagedResult.Data.Select(p => p.ToPeliculaResponse())
        };
    }

    public static PagedResult<Pelicula> ToPagedResult(this PagedResponseDto pagedDto)
    {
        if (pagedDto == null)
        {
            return new PagedResult<Pelicula>
            {
                TotalRecords = 0,
                PageNumber = 1,
                PageSize = 0,
                Data = new List<Pelicula>()
            };
        }
        return new PagedResult<Pelicula>
        {
            PageNumber = pagedDto.PageNumber,
            PageSize = pagedDto.PageSize,
            TotalRecords = pagedDto.TotalRecords,
            Data = pagedDto.Data.ToModel(),

        };
    }

    public static PagedResult<T> ToPagedResult<T>(this PagedResponse<T> pagedResponse)
    {
        return new PagedResult<T>
        {
            PageNumber = pagedResponse.PageNumber,
            PageSize = pagedResponse.PageSize,
            TotalRecords = pagedResponse.TotalRecords,
            TotalPages = pagedResponse.TotalPages,
            Data = pagedResponse.Data.ToList()
        };
    }

    public static PagedResponse<Pelicula> ToPagedResponse(this PagedResponseDto pagedDto)
    {
        return new PagedResponse<Pelicula>
        {
            PageNumber = pagedDto.PageNumber,
            PageSize = pagedDto.PageSize,
            TotalRecords = pagedDto.TotalRecords,
            TotalPages = pagedDto.TotalPages,
            Data = pagedDto.Data.ToModel()
        };
    }
}