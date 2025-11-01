using System.Runtime.Serialization;

namespace peliculaApi.Infrastructure.Dtos;

[DataContract(Name = "Query", Namespace = "http://pelicula-api/pelicula-service")]
public class Query
{
    [DataMember(Order = 1)]
    public string? Title { get; set; }

    [DataMember(Order = 3)]
    public string? Genre { get; set; }

    [DataMember(Order = 4)]
    public int PageSize { get; set; }

    [DataMember(Order = 5)]
    public int PageNumber { get; set; }

    [DataMember(Order = 6)]
    public string OrderBy { get; set; } = string.Empty;

    [DataMember(Order = 7)]
    public string OrderDirection { get; set; } = string.Empty;
}