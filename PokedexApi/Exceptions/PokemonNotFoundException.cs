namespace PokedexApi.Exceptions;
public class PokemonNotFoundException : Exception
{
    public PokemonNotFoundException(Guid id) : base($"pokemon {id} not found")
    {
    }
}