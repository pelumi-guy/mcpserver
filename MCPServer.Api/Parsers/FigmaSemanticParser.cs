using System.Text;
using System.Text.RegularExpressions;
using FigmaMcpServer.Api.DTOs;
using FigmaMcpServer.Api.Models.FigmaApi;

namespace FigmaMcpServer.Api.Parsers;

public sealed partial class FigmaSemanticParser : IFigmaSemanticParser
{
    private const int MaxActionWords = 4;
    private const int MaxActionLength = 36;
    private const int MaxFieldWords = 5;
    private const int MaxFieldLength = 40;

    private static readonly HashSet<string> InputKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "email", "e-mail", "password", "passcode", "phone", "mobile", "username", "user name", "full name",
        "first name", "last name", "address", "search", "otp", "verification code", "pin", "confirm password"
    };

    private static readonly HashSet<string> ActionKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "next", "login", "log in", "signin", "sign in", "signup", "sign up", "create account", "continue", "submit", "register",
        "send", "save", "verify", "done", "get started", "start", "forgot password", "open email app",
        "back to login", "make payment", "proceed", "setup card", "add card", "new request", "contact rider"
    };

    private static readonly HashSet<string> InstructionKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "welcome", "required", "amount is", "up to you", "before you", "we have", "thank you", "details",
        "give us", "enter your", "to reset", "to verify", "to confirm", "you need to"
    };

    private static readonly HashSet<string> NoiseContextKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "status bar", "battery", "wifi", "wi-fi", "signal", "bars/status", "notch", "icon", "nav bar", "home indicator",
        "keyboard", "keypad", "numpad"
    };

    public IReadOnlyList<FigmaSemanticScreenDto> Parse(FigmaFileResponse response, string? nodeId = null)
    {
        var root = response.Document;
        if (root.Children is null || root.Children.Count == 0)
        {
            return [];
        }

        var pagesToScan = ResolvePagesToScan(root, nodeId);
        if (pagesToScan.Count == 0)
        {
            return [];
        }

        var screens = new List<FigmaSemanticScreenDto>();

        foreach (var page in pagesToScan)
        {
            if (page.Children is null || page.Children.Count == 0)
            {
                continue;
            }

            foreach (var node in page.Children)
            {
                if (!IsScreenRoot(node))
                {
                    continue;
                }

                screens.Add(ParseScreen(node));
            }
        }

        return screens;
    }

    private static List<FigmaNode> ResolvePagesToScan(FigmaNode root, string? nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return root.Children ?? [];
        }

        var normalizedNodeId = NormalizeNodeId(nodeId);
        if (string.IsNullOrWhiteSpace(normalizedNodeId))
        {
            throw new ArgumentException("nodeId is invalid.", nameof(nodeId));
        }

        if (!TryFindNode(root, normalizedNodeId, out var target, out var ancestry) || target is null)
        {
            throw new ArgumentException($"Could not find node '{nodeId}' in the Figma file.", nameof(nodeId));
        }

        if (string.Equals(target.Type, "CANVAS", StringComparison.OrdinalIgnoreCase))
        {
            return [target];
        }

        if (IsScreenRoot(target))
        {
            return [CreateSyntheticPage([target])];
        }

        var nearestScreenRoot = ancestry.LastOrDefault(IsScreenRoot);
        if (nearestScreenRoot is not null)
        {
            return [CreateSyntheticPage([nearestScreenRoot])];
        }

        var nearestCanvas = ancestry.LastOrDefault(node =>
            string.Equals(node.Type, "CANVAS", StringComparison.OrdinalIgnoreCase));

        if (nearestCanvas is not null)
        {
            return [nearestCanvas];
        }

        throw new ArgumentException($"Could not resolve a scope for node '{nodeId}'.", nameof(nodeId));
    }

    private static bool TryFindNode(FigmaNode current, string targetId, out FigmaNode? target, out List<FigmaNode> ancestry)
    {
        if (string.Equals(current.Id, targetId, StringComparison.Ordinal))
        {
            target = current;
            ancestry = [current];
            return true;
        }

        if (current.Children is null || current.Children.Count == 0)
        {
            target = null;
            ancestry = [];
            return false;
        }

        foreach (var child in current.Children)
        {
            if (!TryFindNode(child, targetId, out target, out ancestry))
            {
                continue;
            }

            ancestry.Insert(0, current);
            return true;
        }

        target = null;
        ancestry = [];
        return false;
    }

    private static FigmaNode CreateSyntheticPage(IReadOnlyList<FigmaNode> children)
    {
        return new FigmaNode
        {
            Id = "synthetic:scope",
            Name = "Scoped page",
            Type = "CANVAS",
            Children = [..children]
        };
    }

    private static bool IsScreenRoot(FigmaNode node)
    {
        return string.Equals(node.Type, "FRAME", StringComparison.OrdinalIgnoreCase)
            || string.Equals(node.Type, "COMPONENT", StringComparison.OrdinalIgnoreCase)
            || string.Equals(node.Type, "COMPONENT_SET", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeNodeId(string nodeId)
    {
        return nodeId.Trim().Replace("-", ":", StringComparison.Ordinal);
    }

    private static FigmaSemanticScreenDto ParseScreen(FigmaNode screenNode)
    {
        var context = new ScreenParseContext(screenNode.Name);
        Traverse(screenNode, [], context);
        return context.ToDto();
    }

    private static void Traverse(FigmaNode node, IReadOnlyList<string> ancestorNames, ScreenParseContext context)
    {
        var currentAncestors = new List<string>(ancestorNames.Count + 1);
        currentAncestors.AddRange(ancestorNames);
        currentAncestors.Add(node.Name);

        if (string.Equals(node.Type, "TEXT", StringComparison.OrdinalIgnoreCase))
        {
            HandleTextNode(node, currentAncestors, context);
        }

        if (node.Children is null || node.Children.Count == 0)
        {
            return;
        }

        foreach (var child in node.Children)
        {
            Traverse(child, currentAncestors, context);
        }
    }

    private static void HandleTextNode(FigmaNode node, IReadOnlyList<string> ancestors, ScreenParseContext context)
    {
        var text = NormalizeWhitespace(node.Characters);
        if (string.IsNullOrWhiteSpace(text))
        {
            text = NormalizeWhitespace(node.Name);
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if (IsNoiseText(text, ancestors))
        {
            return;
        }

        if (IsActionText(text, node.Name, ancestors))
        {
            context.AddAction(ToSnakeCase(text));
            return;
        }

        if (IsFieldText(text, node.Name, ancestors))
        {
            context.AddField(ToSnakeCase(text));
            return;
        }

        context.AddLabel(text);
    }

    private static bool IsActionText(string text, string nodeName, IReadOnlyList<string> ancestors)
    {
        if (!LooksLikeActionCandidate(text))
        {
            return false;
        }

        if (ContainsKeyword(text, ActionKeywords))
        {
            return true;
        }

        if (ContainsKeyword(nodeName, ActionKeywords))
        {
            return true;
        }

        foreach (var ancestor in ancestors)
        {
            if (ancestor.Contains("btn", StringComparison.OrdinalIgnoreCase)
                || ancestor.Contains("button", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsFieldText(string text, string nodeName, IReadOnlyList<string> ancestors)
    {
        if (!LooksLikeFieldCandidate(text))
        {
            return false;
        }

        if (ContainsKeyword(text, InputKeywords) || ContainsKeyword(nodeName, InputKeywords))
        {
            return true;
        }

        foreach (var ancestor in ancestors)
        {
            if (ancestor.Contains("input", StringComparison.OrdinalIgnoreCase)
                || ancestor.Contains("textfield", StringComparison.OrdinalIgnoreCase)
                || ancestor.Contains("text field", StringComparison.OrdinalIgnoreCase)
                || ancestor.Contains("form", StringComparison.OrdinalIgnoreCase))
            {
                return ContainsKeyword(text, InputKeywords);
            }
        }

        return false;
    }

    private static bool IsNoiseText(string text, IReadOnlyList<string> ancestors)
    {
        if (TimeRegex().IsMatch(text))
        {
            return true;
        }

        if (CurrencyOrNumericRegex().IsMatch(text)
            || UppercaseKeypadRegex().IsMatch(text)
            || SingleSymbolRegex().IsMatch(text))
        {
            return true;
        }

        foreach (var ancestor in ancestors)
        {
            foreach (var keyword in NoiseContextKeywords)
            {
                if (ancestor.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool LooksLikeActionCandidate(string text)
    {
        if (InstructionKeywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        var words = WordRegex().Matches(text).Count;
        if (words == 0 || words > MaxActionWords || text.Length > MaxActionLength)
        {
            return false;
        }

        return true;
    }

    private static bool LooksLikeFieldCandidate(string text)
    {
        if (InstructionKeywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        var words = WordRegex().Matches(text).Count;
        if (words == 0 || words > MaxFieldWords || text.Length > MaxFieldLength)
        {
            return false;
        }

        return true;
    }

    private static bool ContainsKeyword(string source, IReadOnlyCollection<string> keywords)
    {
        foreach (var keyword in keywords)
        {
            if (source.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeWhitespace(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return Regex.Replace(value.Trim(), "\\s+", " ");
    }

    private static string ToSnakeCase(string value)
    {
        var normalized = NormalizeWhitespace(value)
            .Replace("’", "", StringComparison.Ordinal)
            .Replace("'", string.Empty, StringComparison.Ordinal);

        var chars = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (char.IsLetterOrDigit(ch))
            {
                chars.Append(char.ToLowerInvariant(ch));
                continue;
            }

            chars.Append('_');
        }

        var collapsed = Regex.Replace(chars.ToString(), "_+", "_").Trim('_');
        return collapsed;
    }

    [GeneratedRegex("^\\d{1,2}:\\d{2}$", RegexOptions.CultureInvariant)]
    private static partial Regex TimeRegex();

    [GeneratedRegex("^[\\s-]*[\\d,.]+(?:\\s*[A-Z$€¥£₦])?$", RegexOptions.CultureInvariant)]
    private static partial Regex CurrencyOrNumericRegex();

    [GeneratedRegex("^[A-Z]{2,5}$", RegexOptions.CultureInvariant)]
    private static partial Regex UppercaseKeypadRegex();

    [GeneratedRegex("^[^\\p{L}\\p{N}]+$", RegexOptions.CultureInvariant)]
    private static partial Regex SingleSymbolRegex();

    [GeneratedRegex("[A-Za-z0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex WordRegex();

    private sealed class ScreenParseContext
    {
        private readonly HashSet<string> _fields = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _labels = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _actions = new(StringComparer.OrdinalIgnoreCase);

        public ScreenParseContext(string screenName)
        {
            ScreenName = string.IsNullOrWhiteSpace(screenName) ? "Untitled" : screenName;
        }

        public string ScreenName { get; }

        public void AddField(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _fields.Add(value);
            }
        }

        public void AddLabel(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _labels.Add(value);
            }
        }

        public void AddAction(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _actions.Add(value);
            }
        }

        public FigmaSemanticScreenDto ToDto()
        {
            return new FigmaSemanticScreenDto
            {
                Screen = ScreenName,
                Fields = _fields.ToList(),
                Labels = _labels.ToList(),
                Actions = _actions.ToList()
            };
        }
    }
}