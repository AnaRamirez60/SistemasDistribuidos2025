using PokedexApi.Models;

namespace PokedexApi.Dtos;

public class UpdatePokemonRequest
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required StatsRequest Stats { get; set; }
}

