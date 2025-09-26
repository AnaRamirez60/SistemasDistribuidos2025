using Microsoft.AspNetCore.Mvc;
using PokedexApi.Dtos;
using PokedexApi.Services;
using PokedexApi.Mappers;
using System.Drawing.Text;
using System.ServiceModel.Channels;
using PokedexApi.Models;
using PokedexApi.Exceptions;
using PokedexApi.Infrastructure.Soap.Dtos;

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
    public async Task<ActionResult<IList<PokemonResponse>>> GetPokemonsAsync([FromQuery] string name, [FromQuery] string type, [FromQuery] int pageSize, [FromQuery] int pageNumber, [FromQuery] string orderBy, [FromQuery] string orderDirection, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(type))
        {
            return BadRequest(new { Message = "Type query parameter is required" });
        }

    var pokemons = await _pokemonService.GetPokemonsAsync(name, type, pageSize, pageNumber, orderBy, orderDirection, cancellationToken);
        return Ok(pokemons.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<PokemonResponse>> CreatePokemonAsync([FromBody] CreatePokemonRequest createPokemon, CancellationToken cancellationToken)
    {
        try
        {
            if (!IsValidAttack(createPokemon.Stats.Attack))
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

       [HttpPut("{id}")]
public async Task<IActionResult> UpdatePokemonAsync(Guid id, [FromBody] UpdatePokemonRequest pokemon, CancellationToken cancellationToken)
{
    try
    {
        if(!IsValidAttack(pokemon.Stats.Attack))
        {
            return BadRequest(new { Message = "Invalid Attack Value" }); // 400
        }

       await _pokemonService.UpdatePokemonAsync(pokemon.ToModel(id), cancellationToken);
            return NoContent(); // 204
    }
    catch(PokemonNotFoundException)
    {
        return NotFound(); // 404
    }
    catch(PokemonAlreadyExistsException ex)
    {
        return Conflict(new { Message = ex.Message }); // 409
    }
}

    [HttpPatch("{id}")]
    public async Task<ActionResult<PokemonResponse>> PatchPokemonAsync(Guid id, [FromBody] PatchPokemonRequest pokemonRequest, CancellationToken cancellationToken)
    {
         try
    {
        if(pokemonRequest.Attack.HasValue && !IsValidAttack(pokemonRequest.Attack.Value))
        {
            return BadRequest(new { Message = "Invalid Attack Value" }); // 400
        }

        var pokemon = await _pokemonService.PatchPokemonAsync(id, pokemonRequest.Name,pokemonRequest.Type, pokemonRequest.Attack,pokemonRequest.Defense, pokemonRequest.Speed , cancellationToken);
            return Ok(pokemon.ToResponse()); // 200
    }
    catch(PokemonNotFoundException)
    {
        return NotFound(); // 404
    }
    catch(PokemonAlreadyExistsException ex)
    {
        return Conflict(new { Message = ex.Message }); // 409
    }
    
}

    private static bool IsValidAttack(int attack)
    {
        return attack > 0;
    }
    
}