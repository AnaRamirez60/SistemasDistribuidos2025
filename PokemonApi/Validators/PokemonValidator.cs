using PokemonApi.Models;
using PokemonApi.Dtos;
using System.ServiceModel;

namespace PokemonApi.Validators;

public static class PokemonValidator
{
    public static CreatePokemonDto ValidateName(this CreatePokemonDto pokemon) =>
        string.IsNullOrEmpty(pokemon.Name) ? throw new FaultException("Pokemon name is required") : pokemon;
    public static CreatePokemonDto ValidateType(this CreatePokemonDto pokemon) =>
        string.IsNullOrEmpty(pokemon.Type) ? throw new FaultException("Pokemon type is required") : pokemon;
    public static CreatePokemonDto ValidateLevel(this CreatePokemonDto pokemon) =>
        pokemon.Level <= 0 ? throw new FaultException("Pokemon level is required") : pokemon;
}





