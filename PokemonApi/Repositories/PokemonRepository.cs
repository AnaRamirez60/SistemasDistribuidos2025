using Microsoft.EntityFrameworkCore;
using PokemonApi.Infrastructure;
using PokemonApi.Models;
using PokemonApi.Mappers;
using PokemonApi.Dtos;

namespace PokemonApi.Repositories;

public class PokemonRepository : IPokemonRepository
{
    private readonly RelationalDbContext _context;

    public PokemonRepository(RelationalDbContext context)
    {
        _context = context;

    }

    public async Task UpdatePokemonAsync(Pokemon pokemon, CancellationToken cancellationToken)
    {
        _context.Pokemons.Update(pokemon.ToEntity());
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePokemonAsync(Pokemon pokemon, CancellationToken cancellationToken)
    {
        _context.Pokemons.Remove(pokemon.ToEntity());
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Pokemon>> GetPokemonsByNameAsync(string name, CancellationToken cancellationToken)
    {
        //select * from pokemons where name like '%TEXTO%'
        var pokemons = await _context.Pokemons.AsNoTracking().Where(s => s.Name.Contains(name)).ToListAsync(cancellationToken);
        return pokemons.ToModel();
    }

    public async Task<Pokemon> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        //select * from pokemons where id = ID
        var pokemon = await _context.Pokemons.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        return pokemon?.ToModel() ?? throw new InvalidOperationException($"Pokemon with id {id} not found");
    }

    public async Task<Pokemon> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        //select * from pokemons where name like '%TEXTO%'
        var pokemon = await _context.Pokemons.AsNoTracking().FirstOrDefaultAsync(s => s.Name.Contains(name), cancellationToken);
        return pokemon?.ToModel() ?? throw new InvalidOperationException($"Pokemon with name '{name}' not found");
    }

    public async Task<Pokemon> CreateAsync(Pokemon pokemon, CancellationToken cancellationToken)
    {
        var pokemonToCreate = pokemon.ToEntity();
        pokemonToCreate.Id = Guid.NewGuid();
        await _context.Pokemons.AddAsync(pokemonToCreate, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return pokemonToCreate.ToModel();
    }


    public async Task<PagedResponseDto> GetPokemonsAsync(Query query, CancellationToken cancellationToken)
    {

        IQueryable<Infrastructure.Entities.PokemonEntity> queryable = _context.Pokemons.AsNoTracking();


        if (!string.IsNullOrEmpty(query.Type))
        {
            queryable = queryable.Where(p => p.Type.ToLower() == query.Type.ToLower());
        }
        if (!string.IsNullOrEmpty(query.Name))
        {
            queryable = queryable.Where(p => p.Name.ToLower().Contains(query.Name.ToLower()));
        }

        var orderByField = query.OrderBy?.ToLower() ?? string.Empty; 
        var isAscending = query.OrderDirection?.ToLower() == "asc";

        if (orderByField.Contains("name"))
        { 
            queryable = isAscending ? queryable.OrderBy(p => p.Name) : queryable.OrderByDescending(p => p.Name);
        }
        else if (orderByField.Contains("attack"))
        {
            queryable = isAscending ? queryable.OrderBy(p => p.Attack) : queryable.OrderByDescending(p => p.Attack);
        }
        else
        {
            queryable = isAscending ? queryable.OrderBy(p => p.Name) : queryable.OrderByDescending(p => p.Name);
        }
        var totalPokemons = await queryable.CountAsync(cancellationToken);


        var paginacion = await queryable
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);


        var pokemonModels = paginacion.ToModel(); 
        var pokemonDtos = pokemonModels.ToResponseDto(); 

        return new PagedResponseDto
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalRecords = totalPokemons,
            TotalPages = (int)Math.Ceiling(totalPokemons / (double)query.PageSize),
            Data = pokemonDtos.ToList()
        };
    }
}