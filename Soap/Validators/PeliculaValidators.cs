using PeliculaApi.Models;
using PeliculaApi.Dtos;
using System.ServiceModel;

namespace PeliculaApi.Validators;

public static class PeliculaValidator
{
    public static CreatePeliculaDto ValidateTitle(this CreatePeliculaDto pelicula) =>
        string.IsNullOrEmpty(pelicula.Title) ? throw new FaultException("Pelicula title is required") : pelicula;
    public static CreatePeliculaDto ValidateGenre(this CreatePeliculaDto pelicula) =>
        string.IsNullOrEmpty(pelicula.Genre) ? throw new FaultException("Pelicula genre is required") : pelicula;

}



