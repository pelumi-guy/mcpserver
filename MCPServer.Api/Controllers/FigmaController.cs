using FigmaMcpServer.Api.Clients;
using FigmaMcpServer.Api.DTOs;
using FigmaMcpServer.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FigmaMcpServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FigmaController : ControllerBase
{
    private readonly IFigmaParsingService _figmaParsingService;
    private readonly IFigmaTransformService _figmaTransformService;

    public FigmaController(IFigmaParsingService figmaParsingService, IFigmaTransformService figmaTransformService)
    {
        _figmaParsingService = figmaParsingService;
        _figmaTransformService = figmaTransformService;
    }

    [HttpGet("parse")]
    [ProducesResponseType(typeof(FigmaParseResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Parse(
        [FromQuery] string fileKey,
        [FromQuery] string? pageName,
        [FromQuery] string? frameName,
        [FromQuery] bool refreshCache = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
        {
            return BadRequest("Query parameter 'fileKey' is required.");
        }

        try
        {
            var result = await _figmaParsingService.ParseFileAsync(fileKey, new ParseRequestOptions
            {
                PageName = pageName,
                FrameName = frameName,
                RefreshCache = refreshCache
            }, cancellationToken);

            return Ok(result);
        }
        catch (FigmaApiException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                error = ex.Message,
                sourceStatusCode = (int)ex.StatusCode
            });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = ex.Message
            });
        }
    }

    [HttpGet("transform")]
    [ProducesResponseType(typeof(IReadOnlyList<FigmaSemanticScreenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Transform(
        [FromQuery] string fileKey,
        [FromQuery] string? nodeId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
        {
            return BadRequest("Query parameter 'fileKey' is required.");
        }

        try
        {
            var result = await _figmaTransformService.TransformAsync(fileKey, nodeId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (FigmaApiException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                error = ex.Message,
                sourceStatusCode = (int)ex.StatusCode
            });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = ex.Message
            });
        }
    }
}
