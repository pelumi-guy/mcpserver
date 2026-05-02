namespace FigmaMcpServer.Api.DTOs;

public sealed class ParseRequestOptions
{
    public string? PageName { get; init; }

    public string? FrameName { get; init; }

    public bool RefreshCache { get; init; }
}
