namespace peliculaApi.Exceptions;

public class PeliculaNotFoundException : Exception
{
    public PeliculaNotFoundException(Guid id) : base($"pelicula {id} not found")
    {
    }
}

public class PeliculaAlreadyExistsException : Exception
{
    public PeliculaAlreadyExistsException(string peliculaName) : base($"Pelicula {peliculaName} already exists")
    {
    }
}

public class InvalidIdException : Exception
{
    public InvalidIdException(Guid id) : base($"Invalid Id: {id}")
    {
    }
}