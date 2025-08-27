using PokemonApi.Models;

namespace PokemonApi.Repositories;

public interface IPokemonRepository
    {
        Task<Pokemon> GetByNameAsync(string name, CancellationToken cancellationToken);
        Task<Pokemon> CreateAsync(Pokemon pokemon, CancellationToken cancellationToken);
    }
