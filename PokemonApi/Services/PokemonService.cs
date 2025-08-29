using PokemonApi.Dtos;
using PokemonApi.Models;
using PokemonApi.Repositories;
using System.ServiceModel;
using PokemonApi.Mappers;
using PokemonApi.Validators;
using PokemonApi.Services;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace PokemonApi.Services;

public class PokemonService : IPokemonService
{
    private readonly IPokemonRepository _pokemonRepository;

    public PokemonService(IPokemonRepository pokemonRepository)
    {
        _pokemonRepository = pokemonRepository;
    }

    public async Task<IList<PokemonResponseDto>> GetPokemonsByNameAsync(string name, CancellationToken cancellationToken)
    {
        var pokemons = await _pokemonRepository.GetPokemonsByNameAsync(name, cancellationToken);
        return pokemons.ToResponseDto();
    }

    public async Task<PokemonResponseDto> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var pokemon = await _pokemonRepository.GetPokemonByIdAsync(id, cancellationToken);
        return PokemonExists(pokemon) ? pokemon.ToResponseDto() : throw new FaultException("Pokemon not found");
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

    private static bool PokemonExists(Pokemon? pokemon)
    {
        return pokemon is not null;
    }

    private async Task<bool> IsPokemonDuplicated(string name, CancellationToken cancellationToken)
    {
        var pokemon = await _pokemonRepository.GetByNameAsync(name, cancellationToken);
        return pokemon is not null;
    }
}