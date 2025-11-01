using System.Runtime.Serialization;

namespace PokemonApi.Dtos;

[DataContract(Name = "PagedResponseDto", Namespace = "http://pokemon-api/pokemon-service/")]
public class PagedResponseDto
{
    [DataMember(Name = "PageNumber", Order = 1)]
    public int PageNumber { get; set; }

    [DataMember(Name = "PageSize", Order = 2)]
    public int PageSize { get; set; }

    [DataMember(Name = "TotalRecords", Order = 3)]
    public int TotalRecords { get; set; }

    [DataMember(Name = "TotalPages", Order = 4)]
    public int TotalPages { get; set; }

    [DataMember(Name = "Data", Order = 5)]
    public required List<PokemonResponseDto> Data { get; set; }
}