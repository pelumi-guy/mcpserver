using FigmaMcpServer.Api.DTOs;
using FigmaMcpServer.Api.Models.FigmaApi;

namespace FigmaMcpServer.Api.Parsers;

public interface IFigmaSemanticParser
{
    IReadOnlyList<FigmaSemanticScreenDto> Parse(FigmaFileResponse response, string? nodeId = null);
}