using System.Text.Json;
using FigmaMcpServer.Api.Clients;
using FigmaMcpServer.Api.Configuration;
using FigmaMcpServer.Api.DTOs;
using FigmaMcpServer.Api.Parsers;
using FigmaMcpServer.Api.Services;
using Microsoft.AspNetCore.WebUtilities;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FigmaOptions>(builder.Configuration.GetSection(FigmaOptions.SectionName));
builder.Services.AddMemoryCache();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<INodeHandler, InputNodeHandler>();
builder.Services.AddTransient<INodeHandler, ButtonNodeHandler>();
builder.Services.AddTransient<INodeHandler, LabelNodeHandler>();
builder.Services.AddScoped<IFigmaParser, FigmaParser>();
builder.Services.AddScoped<IFigmaSemanticParser, FigmaSemanticParser>();
builder.Services.AddScoped<IFigmaParsingService, FigmaParsingService>();
builder.Services.AddScoped<IFigmaTransformService, FigmaTransformService>();

builder.Services.AddHttpClient<IFigmaApiClient, FigmaApiClient>((serviceProvider, httpClient) =>
    {
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<FigmaOptions>>().Value;
        httpClient.BaseAddress = new Uri(options.ApiBaseUrl);
        httpClient.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddPolicyHandler((serviceProvider, _) =>
    {
        var logger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("FigmaHttpRetryPolicy");

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => (int)response.StatusCode == 429)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, delay, retryAttempt, _) =>
                {
                    logger.LogWarning(
                        "Retrying Figma API call due to status {StatusCode}. Attempt {RetryAttempt}. Delay {DelaySeconds}s",
                        (int?)outcome.Result?.StatusCode,
                        retryAttempt,
                        delay.TotalSeconds);
                });
    });

var app = builder.Build();

if (await TryHandleCliAsync(args, app.Services))
{
    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

static async Task<bool> TryHandleCliAsync(string[] args, IServiceProvider services)
{
    if (args.Length == 0)
    {
        return false;
    }

    var command = args[0];
    if (!string.Equals(command, "parse", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(command, "transform", StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
    {
        Console.Error.WriteLine("Usage: dotnet run -- parse <fileKey> [--page <pageName>] [--frame <frameName>]");
        Console.Error.WriteLine("   or: dotnet run -- transform <fileKey|figmaUrl> [--node-id <nodeId>] [--out <outputPath>]");
        return true;
    }

    using var scope = services.CreateScope();
    if (string.Equals(command, "parse", StringComparison.OrdinalIgnoreCase))
    {
        var pageName = GetNamedArg(args, "--page");
        var frameName = GetNamedArg(args, "--frame");

        var parserService = scope.ServiceProvider.GetRequiredService<IFigmaParsingService>();
        var result = await parserService.ParseFileAsync(args[1], new ParseRequestOptions
        {
            PageName = pageName,
            FrameName = frameName,
            RefreshCache = true
        });

        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
        return true;
    }

    var outputPath = GetNamedArg(args, "--out");
    var nodeIdArg = GetNamedArg(args, "--node-id");
    var (fileKey, nodeIdFromRef) = ParseFigmaReference(args[1]);
    var nodeId = string.IsNullOrWhiteSpace(nodeIdArg) ? nodeIdFromRef : nodeIdArg;

    var transformService = scope.ServiceProvider.GetRequiredService<IFigmaTransformService>();
    var transformed = await transformService.TransformAsync(fileKey, nodeId);
    var transformedJson = JsonSerializer.Serialize(transformed, new JsonSerializerOptions { WriteIndented = true });

    if (!string.IsNullOrWhiteSpace(outputPath))
    {
        var fullPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(fullPath, transformedJson);
        Console.Error.WriteLine($"Wrote semantic transform output to: {fullPath}");
        return true;
    }

    Console.WriteLine(transformedJson);
    return true;
}

static string? GetNamedArg(IReadOnlyList<string> args, string name)
{
    for (var index = 0; index < args.Count - 1; index++)
    {
        if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
        {
            return args[index + 1];
        }
    }

    return null;
}

static (string FileKey, string? NodeId) ParseFigmaReference(string reference)
{
    if (!Uri.TryCreate(reference, UriKind.Absolute, out var uri)
        || !uri.Host.Contains("figma.com", StringComparison.OrdinalIgnoreCase))
    {
        return (reference, null);
    }

    var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
    var designIndex = Array.FindIndex(segments, segment => string.Equals(segment, "design", StringComparison.OrdinalIgnoreCase));
    if (designIndex < 0 || designIndex >= segments.Length - 1)
    {
        return (reference, null);
    }

    var fileKey = segments[designIndex + 1];
    var query = QueryHelpers.ParseQuery(uri.Query);
    var nodeId = query.TryGetValue("node-id", out var nodeIdValues)
        ? nodeIdValues.ToString()
        : null;

    return (fileKey, nodeId);
}
