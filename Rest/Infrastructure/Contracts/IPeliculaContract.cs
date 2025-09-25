using System.ServiceModel;
using peliculaApi.Infrastructure.Dtos;

namespace peliculaApi.Infrastructure.Contracts;

[ServiceContract(Name = "PeliculaService", Namespace = "http://pelicula-api/pelicula-service/")]
public interface IPeliculaContract
{

    [OperationContract]
    Task<PeliculaResponseDto> GetPeliculaById(Guid id, CancellationToken cancellationToken);
}