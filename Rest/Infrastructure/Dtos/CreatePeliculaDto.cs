using System.Runtime.Serialization;

namespace peliculaApi.Infrastructure.Dtos;

[DataContract(Name = "CreatePeliculaDto", Namespace = "http://pelicula-api/pelicula-service")]
public class CreatePeliculaDto
{
    [DataMember(Name = "title", Order = 1)]
    public string? Title { get; set; }

    [DataMember(Name = "director", Order = 2)]
    public string? Director { get; set; }

    [DataMember(Name = "releaseDate", Order = 3)]
    public DateTime ReleaseDate { get; set; }

    [DataMember(Name = "genre", Order = 4)]
    public string? Genre { get; set; }

    [DataMember(Name = "duration", Order = 5)]
    public int Duration { get; set; }
}