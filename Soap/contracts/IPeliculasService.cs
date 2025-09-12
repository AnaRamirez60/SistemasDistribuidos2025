using System.ServiceModel;
using PeliculaApi.Dtos; 
using System.Threading;
using System.Threading.Tasks;

namespace PeliculaApi.Services;

[ServiceContract(Name = "PeliculaService", Namespace = "http://pelicula-api/pelicula-service/")]
public interface IPeliculaService
{
    [OperationContract]
    Task<PeliculaResponseDto> CreatePelicula(CreatePeliculaDto pelicula, CancellationToken cancell);
    [OperationContract]
    Task<PeliculaResponseDto> GetPeliculaByIdAsync(Guid id, CancellationToken cancellationToken);
    [OperationContract]
    Task<IList<PeliculaResponseDto>> GetPeliculasByTitleAsync(string title, CancellationToken cancellationToken);
    [OperationContract]
    Task<DeletePeliculaResponseDto> DeletePelicula(Guid id, CancellationToken cancellationToken);
    [OperationContract]
    Task<IList<PeliculaResponseDto>> GetPeliculasByGenreAsync(string genre, CancellationToken cancellationToken);
     [OperationContract]
    Task<PeliculaResponseDto> UpdatePelicula(UpdatePeliculaDto pelicula, CancellationToken cancellationToken);
}