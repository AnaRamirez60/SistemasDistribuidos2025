using PokemonApi.Models;
using PokemonApi.Infrastructure.Entities;
using PokemonApi.Dtos;
using Microsoft.EntityFrameworkCore;

namespace PokemonApi.Mappers;

public static class PokemonMapper
{
    //Extension method
    public static Pokemon ToModel(this PokemonEntity pokemonEntity)
    {
        if (pokemonEntity is null)
        {
            return null;
        }

        return new Pokemon
        {
            Id = pokemonEntity.Id,
            Name = pokemonEntity.Name,
            Type = pokemonEntity.Type,
            Level = pokemonEntity.Level,
            Stats = new Stats
            {
                Attack = pokemonEntity.Attack,
                Defense = pokemonEntity.Defense,
                Speed = pokemonEntity.Speed,
                HP = pokemonEntity.HP
            }
        };
    }

    public static PokemonEntity ToEntity(this Pokemon pokemon)
    {
        return new PokemonEntity
        {
            Id = pokemon.Id,
            Name = pokemon.Name,
            Type = pokemon.Type,
            Level = pokemon.Level,
            Attack = pokemon.Stats.Attack,
            Defense = pokemon.Stats.Defense,
            Speed = pokemon.Stats.Speed,
            HP = pokemon.Stats.HP
        };
    }

    public static Pokemon ToModel(this CreatePokemonDto requestPokemonDto)
    {
        return new Pokemon
        {
            Name = requestPokemonDto.Name,
            Type = requestPokemonDto.Type,
            Level = requestPokemonDto.Level,
            Stats = new Stats
            {
                Attack = requestPokemonDto.Stats.Attack,
                Defense = requestPokemonDto.Stats.Defense,
                Speed = requestPokemonDto.Stats.Speed,
                HP = requestPokemonDto.Stats.HP
            }
        };
    }

    public static PokemonResponseDto ToResponseDto(this Pokemon pokemon)
    {
        return new PokemonResponseDto
        {
            Id = pokemon.Id,
            Name = pokemon.Name,
            Type = pokemon.Type,
            Level = pokemon.Level,
            Stats = new StatsDto
            {
                Attack = pokemon.Stats.Attack,
                Defense = pokemon.Stats.Defense,
                Speed = pokemon.Stats.Speed,
                HP = pokemon.Stats.HP
            }
        };
    }

    public static IList<PokemonResponseDto> ToResponseDto(this IReadOnlyList<Pokemon> pokemons)
    {
        return pokemons.Select(s => s.ToResponseDto()).ToList();
    }

    public static IReadOnlyList<Pokemon> ToModel(this IReadOnlyList<PokemonEntity> pokemons)
    {
        return pokemons.Select(s => s.ToModel()).ToList();
    }

}