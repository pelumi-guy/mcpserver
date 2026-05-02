namespace FigmaMcpServer.Api.Models.Domain;

public sealed record FigmaNodeModel(
    string Id,
    string Name,
    string Type,
    IReadOnlyList<FigmaNodeModel> Children,
    IReadOnlyDictionary<string, string>? Styles);
