using System.ServiceModel;
using peliculaApi.Infrastructure.Dtos;

namespace peliculaApi.Infrastructure.Contracts;

[ServiceContract(Name = "PeliculaService", Namespace = "http://pelicula-api/pelicula-service/")]
public interface IPeliculaContract
{

    [OperationContract]
    Task<PeliculaResponseDto> GetPeliculaById(Guid id, CancellationToken cancellationToken);

    [OperationContract]
    Task<PeliculaResponseDto> CreatePelicula(CreatePeliculaDto pelicula, CancellationToken cancellationToken);

    [OperationContract]
    Task<IList<PeliculaResponseDto>> GetPeliculasByTitle(string title, CancellationToken cancellationToken);

    [OperationContract]
    Task<DeletePeliculaResponseDto> DeletePelicula(Guid id, CancellationToken cancellationToken);
    [OperationContract]
    Task<PeliculaResponseDto> UpdatePelicula(UpdatePeliculaDto pelicula, CancellationToken cancellationToken);

    [OperationContract]
    Task<PagedResponseDto> GetPeliculas(Query query, CancellationToken cancellationToken);

}