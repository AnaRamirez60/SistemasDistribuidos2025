using Microsoft.AspNetCore.Mvc;
using peliculaApi.Dtos;
using peliculaApi.Services;
using peliculaApi.Mappers;
using System.Drawing.Text;
using System.ServiceModel.Channels;
using peliculaApi.Models;
using peliculaApi.Exceptions;
//git prueba
namespace peliculaApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PeliculasController : ControllerBase
{
    private readonly IPeliculaService _peliculaService;

    public PeliculasController(IPeliculaService peliculaService)
    {
        _peliculaService = peliculaService;
    }

    [HttpGet("{id}", Name = "GetPeliculaByIdAsync")]
public async Task<ActionResult<PeliculaResponse>> GetPeliculaByIdAsync(Guid id, CancellationToken cancellationToken)
{
    try
    {
        var pelicula = await _peliculaService.GetPeliculaByIdAsync(id, cancellationToken);
        return Ok(pelicula.ToResponse());
    }
    catch (InvalidIdException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
    catch (PeliculaNotFoundException ex)
    {
        return NotFound(new { message = ex.Message });
    }
}


}