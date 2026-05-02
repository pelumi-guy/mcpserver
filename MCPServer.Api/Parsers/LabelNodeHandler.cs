using FigmaMcpServer.Api.Models.FigmaApi;

namespace FigmaMcpServer.Api.Parsers;

public sealed class LabelNodeHandler : INodeHandler
{
    public bool CanHandle(FigmaNode node)
    {
        if (!(string.Equals(node.Type, "TEXT", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(node.Characters)))
        {
            return false;
        }

        var source = !string.IsNullOrWhiteSpace(node.Characters) ? node.Characters : node.Name;
        return NodeClassification.IsLikelyLabel(source);
    }

    public void Handle(FigmaNode node, ParseContext context)
    {
        var label = node.Characters;
        if (string.IsNullOrWhiteSpace(label))
        {
            label = node.Name;
        }

        context.AddLabel(label ?? string.Empty);
    }
}
