using PokemonApi.Dtos;
using PokemonApi.Models;
using PokemonApi.Repositories;
using System.ServiceModel;
using PokemonApi.Mappers;
using PokemonApi.Validators;


namespace PokemonApi.Services;

public class PokemonService : IPokemonService
{
    private readonly IPokemonRepository _pokemonRepository;

    public PokemonService(IPokemonRepository pokemonRepository)
    {
        _pokemonRepository = pokemonRepository;
    }
    public async Task<PokemonResponseDto> CreatePokemon(CreatePokemonDto pokemonRequest, CancellationToken cancellationToken)
    {
        //Fluent Methods
        pokemonRequest
            .ValidateName()
            .ValidateType()
            .ValidateLevel();

        if (await IsPokemonDuplicated(pokemonRequest.Name, cancellationToken))
        {
            throw new FaultException("Pokemon already exists");
        }
        
        var pokemon = await _pokemonRepository.CreateAsync(pokemonRequest.ToModel(), cancellationToken);

        return pokemon.ToResponseDto();
    }

    private async Task<bool> IsPokemonDuplicated(string name, CancellationToken cancellationToken)
    {
        var pokemon = await _pokemonRepository.GetByNameAsync(name, cancellationToken);
        return pokemon is not null;
    }
}