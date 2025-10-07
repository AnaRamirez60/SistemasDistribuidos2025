using System.ComponentModel.DataAnnotations;

namespace peliculaApi.Dtos;

public class CreatePeliculaRequest
{
    [Required]
    public string Title { get; set; }
    [Required]
    public string Director { get; set; }
    [Required]
    public int ReleaseYear { get; set; }
    [Required]
    public string Genre { get; set; }
    [Required]
    public int Duration { get; set; }
}