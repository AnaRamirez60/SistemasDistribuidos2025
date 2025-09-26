using PokedexApi.Models;
using PokedexApi.Dtos;

namespace PokedexApi.Gateways;

public interface IPokemonGateway
{
    Task<Pokemon> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IList<Pokemon>> GetPokemonsByNameAsync(string name, CancellationToken cancellationToken);

    Task<Pokemon> CreatePokemonAsync(Pokemon pokemon, CancellationToken cancellationToken);

    Task DeletePokemonAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResponse<Pokemon>> GetPokemonsAsync(string name, string type, int pageSize, int pageNumber, string orderBy, string orderDirection, CancellationToken cancellationToken);

    Task UpdatePokemonAsync(Pokemon pokemon, CancellationToken cancellationToken);
}