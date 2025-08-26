using PokemonApi.Dtos;
using System.Threading;
using System.Threading.Tasks;

namespace PokemonApi.Services
{
    public class PokemonService : IPokemonService
    {
        public Task<PokemonResponseDto> CreatePokemon(CreatePokemonDto pokemon, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }
    }
}