using System.Runtime.Serialization;

namespace PeliculaApi.Dtos;

[DataContract(Name = "DeletePeliculaResponseDto", Namespace = "http://pelicula-api/pelicula-service")]
public class DeletePeliculaResponseDto
{
    [DataMember(Name = "Success", Order = 1)]
    public bool Success { get; set; }
}