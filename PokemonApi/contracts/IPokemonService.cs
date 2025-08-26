using System.ServiceModel;
using PokemonApi.Dtos;
using System.Threading;
using System.Threading.Tasks;
    
namespace PokemonApi.Services;
[ServiceContract(Name = "PokemonService", Namespace = "http://pokemon-api/pokemon-service/")]
public interface IPokemonService
{
    [OperationContract]
    Task<PokemonResponseDto> CreatePokemon(CreatePokemonDto pokemon, CancellationToken cancell);
}