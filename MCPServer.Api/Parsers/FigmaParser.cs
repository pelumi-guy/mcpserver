using FigmaMcpServer.Api.DTOs;
using FigmaMcpServer.Api.Models.FigmaApi;

namespace FigmaMcpServer.Api.Parsers;

public sealed class FigmaParser : IFigmaParser
{
    private readonly IEnumerable<INodeHandler> _handlers;

    public FigmaParser(IEnumerable<INodeHandler> handlers)
    {
        _handlers = handlers;
    }

    public FigmaParseResultDto Parse(FigmaFileResponse response, ParseRequestOptions options)
    {
        var result = new FigmaParseResultDto();
        var root = response.Document;

        if (root.Children is null || root.Children.Count == 0)
        {
            return result;
        }

        foreach (var page in root.Children)
        {
            if (!IsPageSelected(page, options.PageName))
            {
                continue;
            }

            ExtractScreensFromNode(page, result, options);
        }

        return result;
    }

    private void ExtractScreensFromNode(FigmaNode node, FigmaParseResultDto result, ParseRequestOptions options)
    {
        if (IsFrame(node) && IsFrameSelected(node, options.FrameName))
        {
            var screen = new ScreenDto
            {
                Id = node.Id,
                Name = string.IsNullOrWhiteSpace(node.Name) ? "Untitled" : node.Name,
                Type = node.Type,
                RootElementId = node.Id
            };

            var context = new ParseContext(screen);
            Traverse(node, context);
            screen.Elements.AddRange(CollectElements(node));
            result.Screens.Add(screen);
            return;
        }

        if (node.Children is null)
        {
            return;
        }

        foreach (var child in node.Children)
        {
            ExtractScreensFromNode(child, result, options);
        }
    }

    private void Traverse(FigmaNode node, ParseContext context)
    {
        foreach (var handler in _handlers)
        {
            if (!handler.CanHandle(node))
            {
                continue;
            }

            handler.Handle(node, context);
            break;
        }

        if (node.Children is null)
        {
            return;
        }

        foreach (var child in node.Children)
        {
            Traverse(child, context);
        }
    }

    private static bool IsFrame(FigmaNode node)
    {
        return string.Equals(node.Type, "FRAME", StringComparison.OrdinalIgnoreCase)
            || string.Equals(node.Type, "COMPONENT", StringComparison.OrdinalIgnoreCase)
            || string.Equals(node.Type, "COMPONENT_SET", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<ElementDto> CollectElements(FigmaNode root)
    {
        return FlattenElements(root, parentId: null).ToArray();
    }

    private static IEnumerable<ElementDto> FlattenElements(FigmaNode node, string? parentId)
    {
        var orderedChildren = GetOrderedChildren(node).ToArray();

        yield return new ElementDto
        {
            Id = node.Id,
            ParentId = parentId,
            ChildrenIds = orderedChildren.Select(child => child.Id).ToList(),
            Name = node.Name,
            Type = node.Type,
            Role = ResolveRole(node),
            Text = node.Characters,
            ChildCount = orderedChildren.Length,
            Bounds = ToBounds(node.AbsoluteBoundingBox),
            Layout = ToLayout(node),
            Typography = ToTypography(node.Style),
            VisualStyle = ToVisualStyle(node),
            Styles = node.Styles
        };

        foreach (var child in orderedChildren)
        {
            foreach (var element in FlattenElements(child, node.Id))
            {
                yield return element;
            }
        }
    }

    private static IEnumerable<FigmaNode> GetOrderedChildren(FigmaNode node)
    {
        if (node.Children is null)
        {
            return [];
        }

        return node.Children
            .OrderBy(child => child.AbsoluteBoundingBox?.Y ?? float.MaxValue)
            .ThenBy(child => child.AbsoluteBoundingBox?.X ?? float.MaxValue)
            .ThenBy(child => child.Name, StringComparer.OrdinalIgnoreCase);
    }

    private static string ResolveRole(FigmaNode node)
    {
        var source = !string.IsNullOrWhiteSpace(node.Characters) ? node.Characters : node.Name;

        var isContainerType = string.Equals(node.Type, "FRAME", StringComparison.OrdinalIgnoreCase)
            || string.Equals(node.Type, "GROUP", StringComparison.OrdinalIgnoreCase)
            || string.Equals(node.Type, "COMPONENT", StringComparison.OrdinalIgnoreCase)
            || string.Equals(node.Type, "COMPONENT_SET", StringComparison.OrdinalIgnoreCase);

        if (isContainerType)
        {
            return "container";
        }

        if (NodeClassification.IsInputLike(source))
        {
            return "input";
        }

        if (NodeClassification.IsButtonLike(source))
        {
            return "button";
        }

        if (string.Equals(node.Type, "TEXT", StringComparison.OrdinalIgnoreCase))
        {
            return "text";
        }

        if (string.Equals(node.Type, "INSTANCE", StringComparison.OrdinalIgnoreCase))
        {
            return "container";
        }

        return "decorative";
    }

    private static BoundsDto? ToBounds(FigmaRectangle? rectangle)
    {
        if (rectangle is null)
        {
            return null;
        }

        return new BoundsDto
        {
            X = ToDecimal(rectangle.X),
            Y = ToDecimal(rectangle.Y),
            Width = ToDecimal(rectangle.Width),
            Height = ToDecimal(rectangle.Height)
        };
    }

    private static LayoutDto ToLayout(FigmaNode node)
    {
        return new LayoutDto
        {
            LayoutMode = node.LayoutMode,
            PrimaryAxisSizingMode = node.PrimaryAxisSizingMode,
            CounterAxisSizingMode = node.CounterAxisSizingMode,
            PrimaryAxisAlignItems = node.PrimaryAxisAlignItems,
            CounterAxisAlignItems = node.CounterAxisAlignItems,
            ItemSpacing = node.ItemSpacing is null ? null : ToDecimal(node.ItemSpacing.Value),
            Padding = new BoxSpacingDto
            {
                Top = ToDecimal(node.PaddingTop ?? 0),
                Right = ToDecimal(node.PaddingRight ?? 0),
                Bottom = ToDecimal(node.PaddingBottom ?? 0),
                Left = ToDecimal(node.PaddingLeft ?? 0)
            },
            ConstraintHorizontal = node.Constraints?.Horizontal,
            ConstraintVertical = node.Constraints?.Vertical
        };
    }

    private static TypographyDto? ToTypography(FigmaTypeStyle? style)
    {
        if (style is null)
        {
            return null;
        }

        return new TypographyDto
        {
            FontFamily = style.FontFamily,
            FontWeight = style.FontWeight is null ? null : ToDecimal(style.FontWeight.Value),
            FontSize = style.FontSize is null ? null : ToDecimal(style.FontSize.Value),
            LineHeightPx = style.LineHeightPx is null ? null : ToDecimal(style.LineHeightPx.Value),
            LetterSpacing = style.LetterSpacing is null ? null : ToDecimal(style.LetterSpacing.Value),
            TextAlignHorizontal = style.TextAlignHorizontal,
            TextAlignVertical = style.TextAlignVertical
        };
    }

    private static VisualStyleDto ToVisualStyle(FigmaNode node)
    {
        return new VisualStyleDto
        {
            CornerRadius = node.CornerRadius is null ? null : ToDecimal(node.CornerRadius.Value),
            StrokeWeight = node.StrokeWeight is null ? null : ToDecimal(node.StrokeWeight.Value),
            Opacity = node.Opacity is null ? null : ToDecimal(node.Opacity.Value),
            FillColorHex = ToHexColor(GetPrimaryPaintColor(node.Fills)),
            StrokeColorHex = ToHexColor(GetPrimaryPaintColor(node.Strokes))
        };
    }

    private static FigmaColor? GetPrimaryPaintColor(IReadOnlyCollection<FigmaPaint>? paints)
    {
        if (paints is null)
        {
            return null;
        }

        foreach (var paint in paints)
        {
            if (paint.Color is null)
            {
                continue;
            }

            if (!string.Equals(paint.Type, "SOLID", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (paint.Visible is false)
            {
                continue;
            }

            return paint.Color;
        }

        return null;
    }

    private static string? ToHexColor(FigmaColor? color)
    {
        if (color is null)
        {
            return null;
        }

        var r = Math.Clamp((int)Math.Round(color.R * 255), 0, 255);
        var g = Math.Clamp((int)Math.Round(color.G * 255), 0, 255);
        var b = Math.Clamp((int)Math.Round(color.B * 255), 0, 255);
        var a = Math.Clamp((int)Math.Round((color.A ?? 1f) * 255), 0, 255);

        return a >= 255
            ? $"#{r:X2}{g:X2}{b:X2}"
            : $"#{r:X2}{g:X2}{b:X2}{a:X2}";
    }

    private static decimal ToDecimal(float value)
    {
        return Math.Round((decimal)value, 2, MidpointRounding.AwayFromZero);
    }

    private static bool IsPageSelected(FigmaNode page, string? pageName)
    {
        if (string.IsNullOrWhiteSpace(pageName))
        {
            return true;
        }

        return page.Name.Contains(pageName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFrameSelected(FigmaNode frame, string? frameName)
    {
        if (string.IsNullOrWhiteSpace(frameName))
        {
            return true;
        }

        return frame.Name.Contains(frameName, StringComparison.OrdinalIgnoreCase);
    }
}
