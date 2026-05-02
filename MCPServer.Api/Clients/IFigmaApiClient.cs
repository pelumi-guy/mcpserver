using FigmaMcpServer.Api.Models.FigmaApi;

namespace FigmaMcpServer.Api.Clients;

public interface IFigmaApiClient
{
    Task<FigmaFileResponse> GetFileAsync(string fileKey, CancellationToken cancellationToken = default);
}
