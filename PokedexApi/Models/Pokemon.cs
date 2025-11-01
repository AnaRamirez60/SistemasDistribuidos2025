namespace PokedexApi.Models;

public class Pokemon
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public int Level { get; set; }
    public required Stats Stats { get; set; }
}

public class Stats
{
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
}