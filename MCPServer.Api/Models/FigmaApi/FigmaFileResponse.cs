using System.Text.Json.Serialization;

namespace FigmaMcpServer.Api.Models.FigmaApi;

public sealed class FigmaFileResponse
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("lastModified")]
    public string? LastModified { get; init; }

    [JsonPropertyName("document")]
    public FigmaNode Document { get; init; } = new();
}
