namespace PokedexApi.Exceptions;
public class PokemonAlreadyExistsException : Exception
{
    public PokemonAlreadyExistsException(string pokemonName) : base($"pokemon {pokemonName} already exists")
    {
    }
}