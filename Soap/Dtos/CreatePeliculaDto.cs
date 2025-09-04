using System.Runtime.Serialization;

namespace PeliculaApi.Dtos;

[DataContract(Name = "CreatePeliculaDto", Namespace = "http://pelicula-api/pelicula-service/")]
public class CreatePeliculaDto
{
    [DataMember(Name = "title", Order = 1)]
    public string? Title { get; set; }

    [DataMember(Name = "director", Order = 2)]
    public string? Director { get; set; }

    [DataMember(Name = "releaseYear", Order = 3)]
    public int ReleaseYear { get; set; }

    [DataMember(Name = "genre", Order = 4)]
    public string? Genre { get; set; }

    [DataMember(Name = "duration", Order = 5)]
    public int Duration { get; set; }
}
