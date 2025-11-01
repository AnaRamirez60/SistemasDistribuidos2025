using peliculaApi.Models;

namespace peliculaApi.Dtos;

public class UpdatePeliculaRequest
{
    public required string Title { get; set; }
    public required string Director { get; set; }
    public required int ReleaseYear { get; set; }
    public required string Genre { get; set; }
    public required int Duration { get; set; }
}

