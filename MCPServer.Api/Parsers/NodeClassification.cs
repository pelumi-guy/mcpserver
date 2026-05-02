namespace FigmaMcpServer.Api.Parsers;

internal static class NodeClassification
{
    private static readonly string[] InputKeywords =
    [
        "input", "email", "mail", "username", "user name", "phone", "search", "password", "passcode"
    ];

    private static readonly string[] ButtonKeywords =
    [
        "button", "btn", "submit", "login", "sign in", "continue", "next", "register"
    ];

    public static bool IsInputLike(string source)
    {
        return ContainsAny(source, InputKeywords);
    }

    public static bool IsButtonLike(string source)
    {
        return ContainsAny(source, ButtonKeywords);
    }

    public static bool IsLikelyLabel(string source)
    {
        return !IsInputLike(source) && !IsButtonLike(source);
    }

    private static bool ContainsAny(string source, IReadOnlyCollection<string> keywords)
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
}