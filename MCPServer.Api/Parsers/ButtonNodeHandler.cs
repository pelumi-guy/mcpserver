using FigmaMcpServer.Api.Models.FigmaApi;

namespace FigmaMcpServer.Api.Parsers;

public sealed class ButtonNodeHandler : INodeHandler
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "TEXT",
        "INSTANCE",
        "COMPONENT",
        "RECTANGLE"
    };

    public bool CanHandle(FigmaNode node)
    {
        if (!AllowedTypes.Contains(node.Type))
        {
            return false;
        }

        var source = GetSource(node);
        return NodeClassification.IsButtonLike(source);
    }

    public void Handle(FigmaNode node, ParseContext context)
    {
        var action = NormalizeAction(GetSource(node));
        context.AddAction(string.IsNullOrWhiteSpace(action) ? "submit" : action);
    }

    private static string GetSource(FigmaNode node) => !string.IsNullOrWhiteSpace(node.Characters)
        ? node.Characters
        : node.Name;

    private static string NormalizeAction(string action)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            return string.Empty;
        }

        var tokens = action.Split([' ', '-', '_', ':', '/'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join(string.Empty, tokens.Select((token, index) => index == 0
            ? token.ToLowerInvariant()
            : char.ToUpperInvariant(token[0]) + token[1..].ToLowerInvariant()));
    }
}
