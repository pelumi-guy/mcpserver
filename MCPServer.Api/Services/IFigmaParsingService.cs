using FigmaMcpServer.Api.DTOs;

namespace FigmaMcpServer.Api.Services;

public interface IFigmaParsingService
{
    Task<FigmaParseResultDto> ParseFileAsync(string fileKey, ParseRequestOptions options, CancellationToken cancellationToken = default);
}
