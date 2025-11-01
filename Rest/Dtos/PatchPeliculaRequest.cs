namespace peliculaApi.Dtos;

public class PatchPeliculaRequest
{
    public string? Title { get; set; }
    public string? Director { get; set; }
    public int? ReleaseYear { get; set; }
    public string? Genre { get; set; }
    public int? Duration { get; set; }
}
