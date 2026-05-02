namespace FigmaMcpServer.Api.Configuration;

public sealed class FigmaOptions
{
    public const string SectionName = "Figma";

    public string ApiBaseUrl { get; init; } = "https://api.figma.com";

    public string PersonalAccessToken { get; init; } = string.Empty;

    public int CacheMinutes { get; init; } = 5;
}
