using FigmaMcpServer.Api.DTOs;
using FigmaMcpServer.Api.Models.FigmaApi;

namespace FigmaMcpServer.Api.Parsers;

public sealed class InputNodeHandler : INodeHandler
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
        return NodeClassification.IsInputLike(source);
    }

    public void Handle(FigmaNode node, ParseContext context)
    {
        var raw = GetSource(node);
        var type = raw.Contains("password", StringComparison.OrdinalIgnoreCase) || raw.Contains("passcode", StringComparison.OrdinalIgnoreCase)
            ? "password"
            : "text";

        var fieldName = ToFieldName(raw);
        context.AddField(new FieldDto
        {
            Name = string.IsNullOrWhiteSpace(fieldName) ? "field" : fieldName,
            Type = type
        });
    }

    private static string GetSource(FigmaNode node) => !string.IsNullOrWhiteSpace(node.Characters)
        ? node.Characters
        : node.Name;

    private static string ToFieldName(string input)
    {
        var candidate = input.Trim();
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return string.Empty;
        }

        candidate = candidate.Replace("*", string.Empty, StringComparison.Ordinal).Trim();
        var split = candidate.Split([' ', '-', '_', ':', '/'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length == 0)
        {
            return string.Empty;
        }

        if (split.Length == 1)
        {
            return split[0].ToLowerInvariant();
        }

        var normalized = split[0].ToLowerInvariant();
        for (var index = 1; index < split.Length; index++)
        {
            var token = split[index];
            normalized += char.ToUpperInvariant(token[0]) + token[1..].ToLowerInvariant();
        }

        return normalized;
    }
}
