using System.Runtime.Serialization;

namespace PokedexApi.Infrastructure.Soap.Dtos;

[DataContract(Name = "CreatePokemonDto", Namespace = "http://pokemon-api/pokemon-service/")]
public class CreatePokemonDto
{
    [DataMember(Name = "name", Order = 1)]
    public string? Name { get; set; }

    [DataMember(Name = "type", Order = 2)]
    public string? Type { get; set; }

    [DataMember(Name = "level", Order = 3)]
    public int Level { get; set; }

    [DataMember(Name = "stats", Order = 4)]
    public required StatsDto Stats { get; set; }
}
