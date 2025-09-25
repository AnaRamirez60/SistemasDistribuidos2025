using System.Runtime.Serialization;

namespace peliculaApi.Infrastructure.Dtos;

[DataContract(Name = "PeliculaResponseDto", Namespace = "http://pelicula-api/pelicula-service")]

public class PeliculaResponseDto
{
    [DataMember(Name = "id", Order = 1)]
    public Guid Id { get; set; }

    [DataMember(Name = "title", Order = 2)]
    public required string Title { get; set; }

    [DataMember(Name = "director", Order = 3)]
    public string Director { get; set; }

    [DataMember(Name = "releaseYear", Order = 4)]
    public int ReleaseYear { get; set; }

    [DataMember(Name = "genre", Order = 5)]
    public required string Genre { get; set; }

    [DataMember(Name = "duration", Order = 6)]
    public int Duration { get; set; }

}
