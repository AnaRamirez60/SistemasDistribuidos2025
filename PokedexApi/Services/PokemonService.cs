using PokedexApi.Models;
using PokedexApi.Gateways;
using PokedexApi.Exceptions;

namespace PokedexApi.Services;

public class PokemonService : IPokemonService
{
    private readonly IPokemonGateway _pokemonGateway;

    public PokemonService(IPokemonGateway pokemonGateway)
    {
        _pokemonGateway = pokemonGateway;
    }

    public async Task DeletePokemonAsync(Guid id, CancellationToken cancellationToken)
    {
        await _pokemonGateway.DeletePokemonAsync(id, cancellationToken);
    }

    public async Task<Pokemon> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _pokemonGateway.GetPokemonByIdAsync(id, cancellationToken);
    }

    public async Task<IList<Pokemon>> GetPokemonsAsync(string name, string type, int pageSize, int pageNumber, string orderBy, string orderDirection, CancellationToken cancellationToken)
    {
        var pokemons = await _pokemonGateway.GetPokemonsByNameAsync(name, cancellationToken);

        var filtered = pokemons.Where(p => p.Type != null && p.Type.ToLower().Contains(type.ToLower()));

        var sorted = (orderBy.ToLower(), orderDirection.ToLower()) switch
        {
            ("name", "asc") => filtered.OrderBy(p => p.Name),
            ("name", "desc") => filtered.OrderByDescending(p => p.Name),
            ("type", "asc") => filtered.OrderBy(p => p.Type),
            ("type", "desc") => filtered.OrderByDescending(p => p.Type),
            _ => filtered.OrderBy(p => p.Name)
        };
    
    return sorted.ToList();
}

    public async Task<Pokemon> CreatePokemonAsync(Pokemon pokemon, CancellationToken cancellationToken)
    {
        var pokemons = await _pokemonGateway.GetPokemonsByNameAsync(pokemon.Name, cancellationToken);
        if (PokemonExists(pokemons, pokemon.Name))
        {
            throw new PokemonAlreadyExistsException(pokemon.Name);
        }

        return await _pokemonGateway.CreatePokemonAsync(pokemon, cancellationToken);
    }

    private static bool PokemonExists(IList<Pokemon> pokemons, string pokemonNameToSearch)
    {
        return pokemons.Any(s => s.Name.ToLower().Equals(pokemonNameToSearch.ToLower()));
    }
}