using FigmaMcpServer.Api.Clients;
using FigmaMcpServer.Api.Configuration;
using FigmaMcpServer.Api.DTOs;
using FigmaMcpServer.Api.Models.FigmaApi;
using FigmaMcpServer.Api.Parsers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FigmaMcpServer.Api.Services;

public sealed class FigmaParsingService : IFigmaParsingService
{
    private readonly IFigmaApiClient _figmaApiClient;
    private readonly IFigmaParser _figmaParser;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<FigmaParsingService> _logger;
    private readonly FigmaOptions _options;

    public FigmaParsingService(
        IFigmaApiClient figmaApiClient,
        IFigmaParser figmaParser,
        IMemoryCache memoryCache,
        IOptions<FigmaOptions> options,
        ILogger<FigmaParsingService> logger)
    {
        _figmaApiClient = figmaApiClient;
        _figmaParser = figmaParser;
        _memoryCache = memoryCache;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<FigmaParseResultDto> ParseFileAsync(string fileKey, ParseRequestOptions options, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"figma:file:{fileKey}";
        FigmaFileResponse response;

        if (!options.RefreshCache && _memoryCache.TryGetValue(cacheKey, out FigmaFileResponse? cached) && cached is not null)
        {
            _logger.LogInformation("Using cached Figma response for key {FileKey}", fileKey);
            response = cached;
        }
        else
        {
            response = await _figmaApiClient.GetFileAsync(fileKey, cancellationToken);
            _memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(Math.Max(_options.CacheMinutes, 1)));
        }

        try
        {
            var parsed = _figmaParser.Parse(response, options);
            if (parsed.Screens.Count == 0)
            {
                parsed.Warnings.Add("No matching frames/components were found for the provided filters.");
            }

            return parsed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Figma payload for file {FileKey}", fileKey);
            throw new InvalidOperationException("Failed to parse Figma payload.", ex);
        }
    }
}
