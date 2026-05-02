using FigmaMcpServer.Api.DTOs;
using FigmaMcpServer.Api.Models.FigmaApi;

namespace FigmaMcpServer.Api.Parsers;

public interface IFigmaParser
{
    FigmaParseResultDto Parse(FigmaFileResponse response, ParseRequestOptions options);
}
