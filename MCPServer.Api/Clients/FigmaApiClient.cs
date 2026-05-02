using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FigmaMcpServer.Api.Configuration;
using FigmaMcpServer.Api.Models.FigmaApi;
using Microsoft.Extensions.Options;

namespace FigmaMcpServer.Api.Clients;

public sealed class FigmaApiClient : IFigmaApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<FigmaApiClient> _logger;
    private readonly FigmaOptions _options;

    public FigmaApiClient(HttpClient httpClient, IOptions<FigmaOptions> options, ILogger<FigmaApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<FigmaFileResponse> GetFileAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
        {
            throw new ArgumentException("A Figma file key is required.", nameof(fileKey));
        }

        if (string.IsNullOrWhiteSpace(_options.PersonalAccessToken))
        {
            throw new InvalidOperationException("Figma Personal Access Token is not configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/files/{fileKey}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("X-Figma-Token", _options.PersonalAccessToken);

        _logger.LogInformation("Fetching Figma file data for key {FileKey}", fileKey);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Figma API request failed with status {StatusCode}. Body: {Body}", response.StatusCode, content);
            throw new FigmaApiException(response.StatusCode, BuildErrorMessage(response.StatusCode, content));
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<FigmaFileResponse>(content, JsonOptions);
            if (parsed is null)
            {
                throw new FigmaApiException(HttpStatusCode.BadGateway, "Figma API returned an empty response body.");
            }

            return parsed;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Figma API response for key {FileKey}", fileKey);
            throw new FigmaApiException(HttpStatusCode.BadGateway, "Figma API returned a malformed JSON response.");
        }
    }

    private static string BuildErrorMessage(HttpStatusCode statusCode, string content)
    {
        var normalized = string.IsNullOrWhiteSpace(content) ? "No content" : content;

        if (statusCode == (HttpStatusCode)429)
        {
            return $"Figma API rate limit reached: {normalized}";
        }

        return $"Figma API request failed with status {(int)statusCode}: {normalized}";
    }
}
