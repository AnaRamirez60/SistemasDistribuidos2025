using PokedexApi.Infrastructure.Soap.Dtos;
using PokedexApi.Models;
using PokedexApi.Dtos;

namespace PokedexApi.Mappers;

public static class PokemonMapper
{

    public static Pokemon ToModel(this UpdatePokemonRequest pokemon, Guid id)
    {
        return new Pokemon
        {
            Id = id,
            Name = pokemon.Name,
            Type = pokemon.Type,
            Stats = new Stats
            {
                Attack = pokemon.Stats.Attack,
                Defense = pokemon.Stats.Defense,
                Speed = pokemon.Stats.Speed
            }
        };
    }
    public static Pokemon ToModel(this PokemonResponseDto pokemonResponseDto)
    {
        return new Pokemon
        {
            Id = pokemonResponseDto.Id,
            Name = pokemonResponseDto.Name,
            Type = pokemonResponseDto.Type,
            Level = pokemonResponseDto.Level,
            Stats = new Stats
            {
                Attack = pokemonResponseDto.Stats.Attack,
                Defense = pokemonResponseDto.Stats.Defense,
                Speed = pokemonResponseDto.Stats.Speed
            }
        };
    }

    public static IList<Pokemon> ToModel(this IList<PokemonResponseDto> pokemonResponseDtos)
    {
        return pokemonResponseDtos.Select(s => s.ToModel()).ToList();
    }

    public static PokemonResponse ToResponse(this Pokemon pokemon)
    {
        return new PokemonResponse
        {
            Id = pokemon.Id,
            Name = pokemon.Name,
            Type = pokemon.Type,
            Attack = pokemon.Stats.Attack
        };
    }

    public static IList<PokemonResponse> ToResponse(this IList<Pokemon> pokemons)
    {
        return pokemons.Select(s => s.ToResponse()).ToList();
    }
    public static Pokemon ToModel(this CreatePokemonRequest createPokemonRequest)
    {
        return new Pokemon
        {
            Name = createPokemonRequest.Name,
            Type = createPokemonRequest.Type,
            Level = createPokemonRequest.Level,
            Stats = new Stats
            {
                Attack = createPokemonRequest.Stats.Attack,
                Defense = createPokemonRequest.Stats.Defense,
                Speed = createPokemonRequest.Stats.Speed
            }
        };
    }

    public static CreatePokemonDto ToRequest(this Pokemon pokemon)
    {
        return new CreatePokemonDto
        {
            Name = pokemon.Name,
            Type = pokemon.Type,
            Level = pokemon.Level,
            Stats = new StatsDto
            {
                Attack = pokemon.Stats.Attack,
                Defense = pokemon.Stats.Defense,
                Speed = pokemon.Stats.Speed
            }
        };
    }

    public static UpdatePokemonDto ToUpdateRequest(this Pokemon pokemon)
    {
        return new UpdatePokemonDto
        {
            Id = pokemon.Id,
            Name = pokemon.Name,
            Type = pokemon.Type,
            Stats = new StatsDto
            {
                Attack = pokemon.Stats.Attack,
                Defense = pokemon.Stats.Defense,
                Speed = pokemon.Stats.Speed
            }
        };
    }


    public static IEnumerable<PokemonResponse> ToResponse(this IEnumerable<Pokemon> pokemons)
    {
        return pokemons.Select(p => p.ToResponse());
    }

    public static PagedResponse<PokemonResponse> ToResponse(this PagedResult<Pokemon> pagedResult)
    {
        return new PagedResponse<PokemonResponse>
        {
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalRecords = pagedResult.TotalRecords,
            TotalPages = pagedResult.TotalPages,
            Data = pagedResult.Data.ToResponse()
        };
    }

    public static PagedResult<Pokemon> ToPagedResult(this PagedResponseDto pagedDto)
    {
        if (pagedDto == null)
        {
            return new PagedResult<Pokemon>
            {
                TotalRecords = 0,
                PageNumber = 1,
                PageSize = 0,
                Data = new List<Pokemon>()
            };
        }
        return new PagedResult<Pokemon>
        {
            PageNumber = pagedDto.PageNumber,
            PageSize = pagedDto.PageSize,
            TotalRecords = pagedDto.TotalRecords,
            Data = pagedDto.Data.ToModel(),

        };
    }

    public static PagedResult<T> ToPagedResult<T>(this PagedResponse<T> pagedResponse)
    {
        return new PagedResult<T>
        {
            PageNumber = pagedResponse.PageNumber,
            PageSize = pagedResponse.PageSize,
            TotalRecords = pagedResponse.TotalRecords,
            TotalPages = pagedResponse.TotalPages,
            Data = pagedResponse.Data.ToList()
        };
    }

    public static PagedResponse<Pokemon> ToPagedResponse(this PagedResponseDto pagedDto)
    {
        return new PagedResponse<Pokemon>
        {
            PageNumber = pagedDto.PageNumber,
            PageSize = pagedDto.PageSize,
            TotalRecords = pagedDto.TotalRecords,
            TotalPages = pagedDto.TotalPages,
            Data = pagedDto.Data.ToModel()
        };
    }

}