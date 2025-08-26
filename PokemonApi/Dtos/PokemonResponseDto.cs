using System.Runtime.Serialization;

namespace PokemonApi.Dtos;

[DataContract(Name = "PokemonResponseDto", Namespace = "http://pokemon-api/pokemon-service")]

public class PokemonResponseDto
{
    [DataMember(Name = "id", Order = 1)]
    public Guid Id { get; set; }

    [DataMember(Name = "name", Order = 2)]
    public required string Name { get; set; }

    [DataMember(Name = "type", Order = 3)]
    public required string Type { get; set; }

    [DataMember(Name = "level", Order = 4)]
    public int Level { get; set; }

    [DataMember(Name = "stats", Order = 5)]
    public required StatsDto Stats { get; set; }

    [DataMember(Name = "hp", Order = 6)]
    public int HP { get; set; }
}
