namespace FigmaMcpServer.Api.DTOs;

public sealed class FigmaParseResultDto
{
    public string SchemaVersion { get; init; } = "2.0";

    public List<ScreenDto> Screens { get; init; } = [];

    public List<string> Warnings { get; init; } = [];
}

public sealed class ScreenDto
{
    public string Name { get; init; } = string.Empty;

    public string Id { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string RootElementId { get; init; } = string.Empty;

    public List<FieldDto> Fields { get; init; } = [];

    public List<string> Actions { get; init; } = [];

    public List<string> Labels { get; init; } = [];

    public List<ElementDto> Elements { get; init; } = [];
}

public sealed class FieldDto
{
    public string Name { get; init; } = string.Empty;

    public string Type { get; init; } = "text";
}

public sealed class ElementDto
{
    public string Id { get; init; } = string.Empty;

    public string? ParentId { get; init; }

    public List<string> ChildrenIds { get; init; } = [];

    public string Name { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string Role { get; init; } = "unknown";

    public string? Text { get; init; }

    public int ChildCount { get; init; }

    public BoundsDto? Bounds { get; init; }

    public LayoutDto? Layout { get; init; }

    public TypographyDto? Typography { get; init; }

    public VisualStyleDto? VisualStyle { get; init; }

    public Dictionary<string, string>? Styles { get; init; }
}

public sealed class BoundsDto
{
    public decimal X { get; init; }

    public decimal Y { get; init; }

    public decimal Width { get; init; }

    public decimal Height { get; init; }
}

public sealed class LayoutDto
{
    public string? LayoutMode { get; init; }

    public string? PrimaryAxisSizingMode { get; init; }

    public string? CounterAxisSizingMode { get; init; }

    public string? PrimaryAxisAlignItems { get; init; }

    public string? CounterAxisAlignItems { get; init; }

    public decimal? ItemSpacing { get; init; }

    public BoxSpacingDto Padding { get; init; } = new();

    public string? ConstraintHorizontal { get; init; }

    public string? ConstraintVertical { get; init; }
}

public sealed class BoxSpacingDto
{
    public decimal Top { get; init; }

    public decimal Right { get; init; }

    public decimal Bottom { get; init; }

    public decimal Left { get; init; }
}

public sealed class TypographyDto
{
    public string? FontFamily { get; init; }

    public decimal? FontWeight { get; init; }

    public decimal? FontSize { get; init; }

    public decimal? LineHeightPx { get; init; }

    public decimal? LetterSpacing { get; init; }

    public string? TextAlignHorizontal { get; init; }

    public string? TextAlignVertical { get; init; }
}

public sealed class VisualStyleDto
{
    public decimal? CornerRadius { get; init; }

    public decimal? StrokeWeight { get; init; }

    public decimal? Opacity { get; init; }

    public string? FillColorHex { get; init; }

    public string? StrokeColorHex { get; init; }
}
