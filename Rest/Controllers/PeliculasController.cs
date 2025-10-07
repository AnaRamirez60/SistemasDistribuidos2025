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

    [HttpGet]
    public async Task<ActionResult<PagedResponse<PeliculaResponse>>> GetPeliculasAsync(
        [FromQuery] string? title = null,
        [FromQuery] string? genre = null,
        [FromQuery] int pageSize = 10,
        [FromQuery] int pageNumber = 1,
        [FromQuery] string orderBy = "title",
        [FromQuery] string orderDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _peliculaService.GetPeliculasAsync(title ?? string.Empty, genre ?? string.Empty, pageSize, pageNumber, orderBy, orderDirection, cancellationToken);
            
            // Convertir PagedResponse<Pelicula> a PagedResponse<PeliculaResponse>
            var response = new PagedResponse<PeliculaResponse>
            {
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalRecords = result.TotalRecords,
                TotalPages = result.TotalPages,
                Data = result.Data.Select(p => p.ToPeliculaResponse())
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

[HttpPost]
    public async Task<ActionResult<PeliculaResponse>> CreatePeliculaAsync([FromBody] CreatePeliculaRequest createPelicula, CancellationToken cancellationToken)
    {
        try
        {
            if (!IsValidReleaseYear(createPelicula.ReleaseYear))
            {
                return BadRequest(new { Message = "Release Year does not have a valid value" });
            }
            var pelicula = await _peliculaService.CreatePeliculaAsync(createPelicula.ToModel(), cancellationToken);
            return CreatedAtRoute(nameof(GetPeliculaByIdAsync), new { id = pelicula.Id }, pelicula.ToResponse());
        }
        catch (PeliculaAlreadyExistsException e)
        {
            return Conflict(new { Message = e.Message });
        }
       
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePeliculaAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _peliculaService.DeletePeliculaAsync(id, cancellationToken);
            return NoContent();
        }
        catch (PeliculaNotFoundException)
        {
            return NotFound();
        }
    }


       [HttpPut("{id}")]
public async Task<IActionResult> UpdatePeliculaAsync(Guid id, [FromBody] UpdatePeliculaRequest pelicula, CancellationToken cancellationToken)
{
    try
    {
        if(!IsValidReleaseYear(pelicula.ReleaseYear))
        {
            return BadRequest(new { Message = "Invalid Release Year" }); // 400
        }

       await _peliculaService.UpdatePeliculaAsync(pelicula.ToModel(id), cancellationToken);
            return NoContent(); // 204
    }
    catch(PeliculaNotFoundException)
    {
        return NotFound(); // 404
    }
    catch(PeliculaAlreadyExistsException ex)
    {
        return Conflict(new { Message = ex.Message }); // 409
    }
}

    [HttpPatch("{id}")]
    public async Task<ActionResult<PeliculaResponse>> PatchPeliculaAsync(Guid id, [FromBody] PatchPeliculaRequest peliculaRequest, CancellationToken cancellationToken)
    {
         try
    {
        if(peliculaRequest.ReleaseYear.HasValue && !IsValidReleaseYear(peliculaRequest.ReleaseYear.Value))
        {
            return BadRequest(new { Message = "Invalid Release Year" }); // 400
        }

        var pelicula = await _peliculaService.PatchPeliculaAsync(id, peliculaRequest.Title,peliculaRequest.Director, peliculaRequest.ReleaseYear,peliculaRequest.Genre, peliculaRequest.Duration , cancellationToken);
            return Ok(pelicula.ToResponse()); // 200
    }
    catch(PeliculaNotFoundException)
    {
        return NotFound(); // 404
    }
    catch(PeliculaAlreadyExistsException ex)
    {
        return Conflict(new { Message = ex.Message }); // 409
    }
    
}
    private static bool IsValidReleaseYear(int releaseYear)
    {
        return releaseYear > 0;
    }

}