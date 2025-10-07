using Microsoft.EntityFrameworkCore;
using PeliculaApi.Infrastructure;
using PeliculaApi.Models;
using PeliculaApi.Mappers;
using PeliculaApi.Dtos;

namespace PeliculaApi.Repositories;

public class PeliculaRepository : IPeliculaRepository
{
    private readonly RelationalDbContext _context;

    public PeliculaRepository(RelationalDbContext context)
    {
        _context = context;

    }

    public async Task UpdatePeliculaAsync(Pelicula pelicula, CancellationToken cancellationToken)
    {
        _context.Peliculas.Update(pelicula.ToEntity());
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePeliculaAsync(Pelicula pelicula, CancellationToken cancellationToken)
    {
        _context.Peliculas.Remove(pelicula.ToEntity());
        await _context.SaveChangesAsync(cancellationToken);
    }


    public async Task<IReadOnlyList<Pelicula>> GetPeliculasByTitleAsync(string title, CancellationToken cancellationToken)
    {
        var peliculas = await _context.Peliculas.AsNoTracking().Where(s => s.Title.Contains(title)).ToListAsync(cancellationToken);
        return peliculas.ToModel();
    }

    public async Task<IReadOnlyList<Pelicula>> GetPeliculasByGenreAsync(string genre, CancellationToken cancellationToken)
    {
        var peliculas = await _context.Peliculas.AsNoTracking().Where(s => s.Genre.Contains(genre)).ToListAsync(cancellationToken);
        return peliculas.ToModel();
    }

    public async Task<Pelicula> GetPeliculaByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var pelicula = await _context.Peliculas.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        return pelicula.ToModel();
    }

    public async Task<Pelicula> CreateAsync(Pelicula pelicula, CancellationToken cancellationToken)
    {
        var peliculaToCreate = pelicula.ToEntity();
        peliculaToCreate.Id = Guid.NewGuid();
        await _context.Peliculas.AddAsync(peliculaToCreate, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return peliculaToCreate.ToModel();
    }

    public async Task<PagedResponseDto> GetPeliculasAsync(Query query, CancellationToken cancellationToken)
    {
        // Debug: Log de parámetros recibidos
        Console.WriteLine($"Query recibida - Title: '{query.Title}', Type: '{query.Type}', PageNumber: {query.PageNumber}, PageSize: {query.PageSize}, OrderBy: '{query.OrderBy}', OrderDirection: '{query.OrderDirection}'");
        
        IQueryable<Infrastructure.Entities.PeliculaEntity> queryable = _context.Peliculas.AsNoTracking();

        if (!string.IsNullOrEmpty(query.Type))
        {
            queryable = queryable.Where(p => p.Genre.ToLower() == query.Type.ToLower());
        }
        if (!string.IsNullOrEmpty(query.Title))
        {
            queryable = queryable.Where(p => p.Title.ToLower().Contains(query.Title.ToLower()));
        }

        var orderByField = query.OrderBy?.ToLower() ?? string.Empty; 
        var isAscending = query.OrderDirection?.ToLower() == "asc";

        Console.WriteLine($"OrderBy procesado: '{orderByField}', IsAscending: {isAscending}");

        if (orderByField.Contains("title"))
        { 
            queryable = isAscending ? queryable.OrderBy(p => p.Title) : queryable.OrderByDescending(p => p.Title);
        }
        else if (orderByField.Contains("genre"))
        {
            queryable = isAscending ? queryable.OrderBy(p => p.Genre) : queryable.OrderByDescending(p => p.Genre);
        }
        else
        {
            queryable = isAscending ? queryable.OrderBy(p => p.Title) : queryable.OrderByDescending(p => p.Title);
        }
        
        var totalPeliculas = await queryable.CountAsync(cancellationToken);
        Console.WriteLine($"Total películas encontradas: {totalPeliculas}");

        var skipCount = (query.PageNumber - 1) * query.PageSize;
        Console.WriteLine($"Skip: {skipCount}, Take: {query.PageSize}");

        var paginacion = await queryable
            .Skip(skipCount)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        Console.WriteLine($"Registros obtenidos: {paginacion.Count}");

        var peliculaModels = paginacion.ToModel();
        var peliculaDtos = peliculaModels.ToResponseDto();

        return new PagedResponseDto
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalRecords = totalPeliculas,
            TotalPages = (int)Math.Ceiling(totalPeliculas / (double)query.PageSize),
            Data = peliculaDtos.ToList()
        };
    }
}