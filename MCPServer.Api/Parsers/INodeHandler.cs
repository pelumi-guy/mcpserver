using FigmaMcpServer.Api.Models.FigmaApi;

namespace FigmaMcpServer.Api.Parsers;

public interface INodeHandler
{
    bool CanHandle(FigmaNode node);

    void Handle(FigmaNode node, ParseContext context);
}
