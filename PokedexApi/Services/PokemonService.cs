using PokedexApi.Models;
using PokedexApi.Gateways;
using PokedexApi.Exceptions;
using PokedexApi.Mappers;

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

    public async Task<PagedResult<Pokemon>> GetPokemonsAsync(
    string name,
    string type,
    int pageSize,
    int pageNumber,
    string orderBy,
    string orderDirection,
    CancellationToken cancellationToken)
{
    var result = await _pokemonGateway.GetPokemonsAsync(name, type, pageSize, pageNumber, orderBy, orderDirection, cancellationToken);
    return result.ToPagedResult();
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

    public async Task UpdatePokemonAsync(Pokemon pokemon, CancellationToken cancellationToken)
    {
        await _pokemonGateway.UpdatePokemonAsync(pokemon, cancellationToken);
    }

    public async Task<Pokemon> PatchPokemonAsync(Guid id, string? name, string? type, int? attack, int? defense, int? speed, CancellationToken cancellationToken)
    {
        var pokemon = await _pokemonGateway.GetPokemonByIdAsync(id, cancellationToken);
        if (pokemon == null)
        {
            throw new PokemonNotFoundException(id);
        }

        pokemon.Name = name ?? pokemon.Name;
        pokemon.Type = type ?? pokemon.Type;
        pokemon.Stats.Attack = attack ?? pokemon.Stats.Attack;
        pokemon.Stats.Defense = defense ?? pokemon.Stats.Defense;
        pokemon.Stats.Speed = speed ?? pokemon.Stats.Speed;

        await _pokemonGateway.UpdatePokemonAsync(pokemon, cancellationToken);
        return pokemon;
    }

    private static bool PokemonExists(IList<Pokemon> pokemons, string pokemonNameToSearch)
    {
        return pokemons.Any(s => s.Name.ToLower().Equals(pokemonNameToSearch.ToLower()));
    }
}