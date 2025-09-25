using Microsoft.AspNetCore.Mvc;
using PokedexApi.Dtos;
using PokedexApi.Services;
using PokedexApi.Mappers;
using System.Drawing.Text;
using System.ServiceModel.Channels;
using PokedexApi.Models;
using PokedexApi.Exceptions;

namespace PokedexApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PokemonsController : ControllerBase
{
    private readonly IPokemonService _pokemonService;

    public PokemonsController(IPokemonService pokemonService)
    {
        _pokemonService = pokemonService;
    }

    //localhost:PORT/api/v1/pokemons/
    [HttpGet("{id}", Name = "GetPokemonByIdAsync")]
    public async Task<ActionResult<PokemonResponse>> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var pokemon = await _pokemonService.GetPokemonByIdAsync(id, cancellationToken);
        return pokemon is null ? NotFound() : Ok(pokemon.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IList<PokemonResponse>>> GetPokemonsAsync([FromQuery] string name, [FromQuery] string type, [FromQuery] int? pageSize = 10, [FromQuery] int? pageNumber = 1, [FromQuery] string orderBy = "name", [FromQuery] string orderDirection = "asc", CancellationToken cancellationToken= default)
    {
        if (string.IsNullOrEmpty(type))
        {
            return BadRequest(new { Message = "Type query parameter is required" });
        }

        int finalPageSize = pageSize ?? 10;
        int finalPageNumber = pageNumber ?? 1;
        string finalOrderBy = string.IsNullOrEmpty(orderBy) ? "name" : orderBy;
        string finalOrderDirection = string.IsNullOrEmpty(orderDirection) ? "asc" : orderDirection.ToLower();

         if (finalOrderDirection != "asc" && finalOrderDirection != "desc")
    {
        return BadRequest(new { Message = "OrderDirection must be 'asc' or 'desc'" });
    }
        var allPokemons = await _pokemonService.GetPokemonsAsync(name, type, finalPageSize, finalPageNumber, finalOrderBy, finalOrderDirection,cancellationToken);
        var totalRecords = allPokemons.Count;
        var totalPages = (int)Math.Ceiling(totalRecords / (double)finalPageSize);
        var sorted = finalOrderDirection == "asc"
        ? allPokemons.OrderBy(s => orderBy.ToLower() == "type" ? s.Type : s.Name).ToList(): allPokemons.OrderByDescending(s => orderBy.ToLower() == "type" ? s.Type : s.Name).ToList();

        var pagedData = sorted
        .Skip((finalPageNumber - 1) * finalPageSize)
        .Take(finalPageSize)
        .ToResponse(); 

            var response = new PagedResponse<PokemonResponse>
    {
        PageNumber = finalPageNumber,
        PageSize = finalPageSize,
        TotalRecords = totalRecords,
        TotalPages = totalPages,
        Data = pagedData
    };

    return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<PokemonResponse>> CreatePokemonAsync([FromBody] CreatePokemonRequest createPokemon, CancellationToken cancellationToken)
    {
        try
        {
            if (!IsValidAttack(createPokemon))
            {
                return BadRequest(new { Message = "Attack does not have a valid value" });
            }
            var pokemon = await _pokemonService.CreatePokemonAsync(createPokemon.ToModel(), cancellationToken);
            return CreatedAtRoute(nameof(GetPokemonByIdAsync), new { id = pokemon.Id }, pokemon.ToResponse());
        }
        catch (PokemonAlreadyExistsException e)
        {
            return Conflict(new { Message = e.Message });
        }
       
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePokemonAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _pokemonService.DeletePokemonAsync(id, cancellationToken);
            return NoContent(); 
        }
        catch (PokemonNotFoundException)
        {
            return NotFound();
        }
    }

        private static bool IsValidAttack(CreatePokemonRequest createPokemon)
    {
        return createPokemon.Stats.Attack > 0;
    }
}