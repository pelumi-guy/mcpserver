using System.Text.Json.Serialization;

namespace FigmaMcpServer.Api.Models.FigmaApi;

public sealed class FigmaNode
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("characters")]
    public string? Characters { get; init; }

    [JsonPropertyName("children")]
    public List<FigmaNode>? Children { get; init; }

    [JsonPropertyName("styles")]
    public Dictionary<string, string>? Styles { get; init; }

    [JsonPropertyName("absoluteBoundingBox")]
    public FigmaRectangle? AbsoluteBoundingBox { get; init; }

    [JsonPropertyName("layoutMode")]
    public string? LayoutMode { get; init; }

    [JsonPropertyName("primaryAxisSizingMode")]
    public string? PrimaryAxisSizingMode { get; init; }

    [JsonPropertyName("counterAxisSizingMode")]
    public string? CounterAxisSizingMode { get; init; }

    [JsonPropertyName("primaryAxisAlignItems")]
    public string? PrimaryAxisAlignItems { get; init; }

    [JsonPropertyName("counterAxisAlignItems")]
    public string? CounterAxisAlignItems { get; init; }

    [JsonPropertyName("itemSpacing")]
    public float? ItemSpacing { get; init; }

    [JsonPropertyName("paddingTop")]
    public float? PaddingTop { get; init; }

    [JsonPropertyName("paddingRight")]
    public float? PaddingRight { get; init; }

    [JsonPropertyName("paddingBottom")]
    public float? PaddingBottom { get; init; }

    [JsonPropertyName("paddingLeft")]
    public float? PaddingLeft { get; init; }

    [JsonPropertyName("cornerRadius")]
    public float? CornerRadius { get; init; }

    [JsonPropertyName("strokeWeight")]
    public float? StrokeWeight { get; init; }

    [JsonPropertyName("opacity")]
    public float? Opacity { get; init; }

    [JsonPropertyName("fills")]
    public List<FigmaPaint>? Fills { get; init; }

    [JsonPropertyName("strokes")]
    public List<FigmaPaint>? Strokes { get; init; }

    [JsonPropertyName("constraints")]
    public FigmaConstraints? Constraints { get; init; }

    [JsonPropertyName("style")]
    public FigmaTypeStyle? Style { get; init; }
}

public sealed class FigmaRectangle
{
    [JsonPropertyName("x")]
    public float X { get; init; }

    [JsonPropertyName("y")]
    public float Y { get; init; }

    [JsonPropertyName("width")]
    public float Width { get; init; }

    [JsonPropertyName("height")]
    public float Height { get; init; }
}

public sealed class FigmaPaint
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("visible")]
    public bool? Visible { get; init; }

    [JsonPropertyName("opacity")]
    public float? Opacity { get; init; }

    [JsonPropertyName("color")]
    public FigmaColor? Color { get; init; }
}

public sealed class FigmaColor
{
    [JsonPropertyName("r")]
    public float R { get; init; }

    [JsonPropertyName("g")]
    public float G { get; init; }

    [JsonPropertyName("b")]
    public float B { get; init; }

    [JsonPropertyName("a")]
    public float? A { get; init; }
}

public sealed class FigmaConstraints
{
    [JsonPropertyName("horizontal")]
    public string? Horizontal { get; init; }

    [JsonPropertyName("vertical")]
    public string? Vertical { get; init; }
}

public sealed class FigmaTypeStyle
{
    [JsonPropertyName("fontFamily")]
    public string? FontFamily { get; init; }

    [JsonPropertyName("fontWeight")]
    public float? FontWeight { get; init; }

    [JsonPropertyName("fontSize")]
    public float? FontSize { get; init; }

    [JsonPropertyName("lineHeightPx")]
    public float? LineHeightPx { get; init; }

    [JsonPropertyName("letterSpacing")]
    public float? LetterSpacing { get; init; }

    [JsonPropertyName("textAlignHorizontal")]
    public string? TextAlignHorizontal { get; init; }

    [JsonPropertyName("textAlignVertical")]
    public string? TextAlignVertical { get; init; }
}
