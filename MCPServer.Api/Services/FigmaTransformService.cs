using FigmaMcpServer.Api.Clients;
using FigmaMcpServer.Api.Configuration;
using FigmaMcpServer.Api.DTOs;
using FigmaMcpServer.Api.Models.FigmaApi;
using FigmaMcpServer.Api.Parsers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FigmaMcpServer.Api.Services;

public sealed class FigmaTransformService : IFigmaTransformService
{
    private readonly IFigmaApiClient _figmaApiClient;
    private readonly IFigmaSemanticParser _semanticParser;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<FigmaTransformService> _logger;
    private readonly FigmaOptions _options;

    public FigmaTransformService(
        IFigmaApiClient figmaApiClient,
        IFigmaSemanticParser semanticParser,
        IMemoryCache memoryCache,
        IOptions<FigmaOptions> options,
        ILogger<FigmaTransformService> logger)
    {
        _figmaApiClient = figmaApiClient;
        _semanticParser = semanticParser;
        _memoryCache = memoryCache;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<FigmaSemanticScreenDto>> TransformAsync(string fileKey, string? nodeId = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"figma:file:{fileKey}";
        FigmaFileResponse response;

        if (_memoryCache.TryGetValue(cacheKey, out FigmaFileResponse? cached) && cached is not null)
        {
            _logger.LogInformation("Using cached Figma response for transform key {FileKey}", fileKey);
            response = cached;
        }
        else
        {
            response = await _figmaApiClient.GetFileAsync(fileKey, cancellationToken);
            _memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(Math.Max(_options.CacheMinutes, 1)));
        }

        var parsed = _semanticParser.Parse(response, nodeId);
        return parsed
            .Where(screen => screen.Fields.Count > 0 || screen.Labels.Count > 0 || screen.Actions.Count > 0)
            .ToList();
    }
}