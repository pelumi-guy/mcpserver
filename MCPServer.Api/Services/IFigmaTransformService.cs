using FigmaMcpServer.Api.DTOs;

namespace FigmaMcpServer.Api.Services;

public interface IFigmaTransformService
{
    Task<IReadOnlyList<FigmaSemanticScreenDto>> TransformAsync(string fileKey, string? nodeId = null, CancellationToken cancellationToken = default);
}