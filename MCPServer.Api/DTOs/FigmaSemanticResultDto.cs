using System.Text.Json.Serialization;

namespace FigmaMcpServer.Api.DTOs;

public sealed class FigmaSemanticScreenDto
{
    [JsonPropertyName("screen")]
    public string Screen { get; init; } = string.Empty;

    [JsonPropertyName("fields")]
    public List<string> Fields { get; init; } = [];

    [JsonPropertyName("labels")]
    public List<string> Labels { get; init; } = [];

    [JsonPropertyName("actions")]
    public List<string> Actions { get; init; } = [];
}