using FigmaMcpServer.Api.DTOs;
using FigmaMcpServer.Api.Models.FigmaApi;
using FigmaMcpServer.Api.Parsers;

namespace FigmaMcpServer.Tests.Parsers;

public sealed class FigmaParserTests
{
    [Fact]
    public void Parse_ExtractsScreenFieldsAndActions()
    {
        var parser = CreateParser();
        var response = BuildSampleResponse();

        var result = parser.Parse(response, new ParseRequestOptions());

        Assert.Equal(2, result.Screens.Count);
        var login = Assert.Single(result.Screens.Where(screen => screen.Name == "Login"));
        Assert.Equal("1:1", login.Id);
        Assert.Equal("FRAME", login.Type);
        Assert.Equal("1:1", login.RootElementId);
        Assert.Equal("Login", login.Name);
        Assert.Contains(login.Fields, field => field.Name.Contains("email", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(login.Fields, field => field.Type == "password");
        Assert.Contains("login", login.Actions);
        Assert.Contains("Welcome Back", login.Labels);
        Assert.NotEmpty(login.Elements);
        Assert.Contains(login.Elements, element => element.Id == "1:4" && element.Type == "INSTANCE");

        var rootElement = Assert.Single(login.Elements.Where(element => element.Id == "1:1"));
        Assert.Null(rootElement.ParentId);
        Assert.Equal("container", rootElement.Role);
        Assert.NotNull(rootElement.Bounds);
        Assert.Equal(24m, rootElement.Bounds!.X);
        Assert.Equal(120m, rootElement.Bounds!.Y);
        Assert.Equal("#FFFFFF", rootElement.VisualStyle?.FillColorHex);

        var buttonElement = Assert.Single(login.Elements.Where(element => element.Id == "1:4"));
        Assert.Equal("1:1", buttonElement.ParentId);
        Assert.Equal("button", buttonElement.Role);
        Assert.Equal("#2563EB", buttonElement.VisualStyle?.FillColorHex);

        var headingElement = Assert.Single(login.Elements.Where(element => element.Id == "1:5"));
        Assert.Equal("text", headingElement.Role);
        Assert.Equal("Sora", headingElement.Typography?.FontFamily);
        Assert.Equal(28m, headingElement.Typography?.FontSize);
    }

    [Fact]
    public void Parse_FiltersByFrameName()
    {
        var parser = CreateParser();
        var response = BuildSampleResponse();

        var result = parser.Parse(response, new ParseRequestOptions { FrameName = "Other" });

        Assert.Single(result.Screens);
        Assert.Equal("Other", result.Screens[0].Name);
    }

    [Fact]
    public void Parse_ExtractsComponentAsScreenRoot()
    {
        var parser = CreateParser();
        var response = BuildComponentResponse();

        var result = parser.Parse(response, new ParseRequestOptions());

        var screen = Assert.Single(result.Screens);
        Assert.Equal("AuthCard", screen.Name);
        Assert.Equal("COMPONENT", screen.Type);
        Assert.Contains(screen.Actions, action => action == "continue");
        Assert.Equal("3:1", screen.RootElementId);
    }

    [Fact]
    public void Parse_DoesNotAddActionWordsToLabels()
    {
        var parser = CreateParser();
        var response = BuildLabelGuardResponse();

        var result = parser.Parse(response, new ParseRequestOptions());

        var screen = Assert.Single(result.Screens);
        Assert.DoesNotContain("Login", screen.Labels);
        Assert.DoesNotContain("Continue", screen.Labels);
        Assert.Contains("Welcome", screen.Labels);
    }

    [Fact]
    public void Parse_ProducesStableTopDownElementOrder()
    {
        var parser = CreateParser();
        var response = BuildOrderingResponse();

        var result = parser.Parse(response, new ParseRequestOptions());

        var screen = Assert.Single(result.Screens);
        var childIds = screen.Elements
            .Where(element => element.ParentId == "5:1")
            .Select(element => element.Id)
            .ToArray();

        Assert.Equal(["5:2", "5:3", "5:4"], childIds);
    }

    private static IFigmaParser CreateParser()
    {
        var handlers = new INodeHandler[]
        {
            new InputNodeHandler(),
            new ButtonNodeHandler(),
            new LabelNodeHandler()
        };

        return new FigmaParser(handlers);
    }

    private static FigmaFileResponse BuildSampleResponse()
    {
        return new FigmaFileResponse
        {
            Name = "Auth",
            Document = new FigmaNode
            {
                Id = "0:0",
                Name = "Document",
                Type = "DOCUMENT",
                Children =
                [
                    new FigmaNode
                    {
                        Id = "0:1",
                        Name = "Page 1",
                        Type = "CANVAS",
                        Children =
                        [
                            new FigmaNode
                            {
                                Id = "1:1",
                                Name = "Login",
                                Type = "FRAME",
                                Children =
                                [
                                    new FigmaNode { Id = "1:2", Name = "Email Input", Type = "TEXT", Characters = "Email" },
                                    new FigmaNode
                                    {
                                        Id = "1:3",
                                        Name = "Password Input",
                                        Type = "TEXT",
                                        Characters = "Password",
                                        AbsoluteBoundingBox = new FigmaRectangle { X = 40, Y = 220, Width = 320, Height = 24 }
                                    },
                                    new FigmaNode
                                    {
                                        Id = "1:4",
                                        Name = "Login Button",
                                        Type = "INSTANCE",
                                        Characters = "Login",
                                        AbsoluteBoundingBox = new FigmaRectangle { X = 40, Y = 280, Width = 320, Height = 48 },
                                        Fills = [new FigmaPaint { Type = "SOLID", Visible = true, Color = new FigmaColor { R = 0.145f, G = 0.388f, B = 0.922f, A = 1f } }]
                                    },
                                    new FigmaNode
                                    {
                                        Id = "1:5",
                                        Name = "Headline",
                                        Type = "TEXT",
                                        Characters = "Welcome Back",
                                        Style = new FigmaTypeStyle
                                        {
                                            FontFamily = "Sora",
                                            FontSize = 28,
                                            FontWeight = 700,
                                            LineHeightPx = 32,
                                            LetterSpacing = 0
                                        }
                                    }
                                ]
                                ,
                                AbsoluteBoundingBox = new FigmaRectangle { X = 24, Y = 120, Width = 390, Height = 720 },
                                LayoutMode = "VERTICAL",
                                ItemSpacing = 16,
                                PaddingTop = 24,
                                PaddingRight = 24,
                                PaddingBottom = 24,
                                PaddingLeft = 24,
                                Fills = [new FigmaPaint { Type = "SOLID", Visible = true, Color = new FigmaColor { R = 1f, G = 1f, B = 1f, A = 1f } }]
                            },
                            new FigmaNode
                            {
                                Id = "2:1",
                                Name = "Other",
                                Type = "FRAME",
                                Children =
                                [
                                    new FigmaNode { Id = "2:2", Name = "Register Button", Type = "INSTANCE", Characters = "Register" }
                                ]
                            }
                        ]
                    }
                ]
            }
        };
    }

    private static FigmaFileResponse BuildComponentResponse()
    {
        return new FigmaFileResponse
        {
            Name = "Auth Component",
            Document = new FigmaNode
            {
                Id = "0:0",
                Name = "Document",
                Type = "DOCUMENT",
                Children =
                [
                    new FigmaNode
                    {
                        Id = "0:1",
                        Name = "Page 1",
                        Type = "CANVAS",
                        Children =
                        [
                            new FigmaNode
                            {
                                Id = "3:1",
                                Name = "AuthCard",
                                Type = "COMPONENT",
                                AbsoluteBoundingBox = new FigmaRectangle { X = 0, Y = 0, Width = 320, Height = 280 },
                                Children =
                                [
                                    new FigmaNode { Id = "3:2", Name = "Continue Button", Type = "TEXT", Characters = "Continue" }
                                ]
                            }
                        ]
                    }
                ]
            }
        };
    }

    private static FigmaFileResponse BuildLabelGuardResponse()
    {
        return new FigmaFileResponse
        {
            Name = "Label Guard",
            Document = new FigmaNode
            {
                Id = "0:0",
                Name = "Document",
                Type = "DOCUMENT",
                Children =
                [
                    new FigmaNode
                    {
                        Id = "0:1",
                        Name = "Page 1",
                        Type = "CANVAS",
                        Children =
                        [
                            new FigmaNode
                            {
                                Id = "4:1",
                                Name = "Prompt",
                                Type = "FRAME",
                                Children =
                                [
                                    new FigmaNode { Id = "4:2", Name = "Primary CTA", Type = "TEXT", Characters = "Login" },
                                    new FigmaNode { Id = "4:3", Name = "Secondary CTA", Type = "TEXT", Characters = "Continue" },
                                    new FigmaNode { Id = "4:4", Name = "Header", Type = "TEXT", Characters = "Welcome" }
                                ]
                            }
                        ]
                    }
                ]
            }
        };
    }

    private static FigmaFileResponse BuildOrderingResponse()
    {
        return new FigmaFileResponse
        {
            Name = "Order",
            Document = new FigmaNode
            {
                Id = "0:0",
                Name = "Document",
                Type = "DOCUMENT",
                Children =
                [
                    new FigmaNode
                    {
                        Id = "0:1",
                        Name = "Page 1",
                        Type = "CANVAS",
                        Children =
                        [
                            new FigmaNode
                            {
                                Id = "5:1",
                                Name = "OrderScreen",
                                Type = "FRAME",
                                Children =
                                [
                                    new FigmaNode { Id = "5:4", Name = "Bottom", Type = "TEXT", Characters = "Bottom", AbsoluteBoundingBox = new FigmaRectangle { X = 20, Y = 300, Width = 100, Height = 20 } },
                                    new FigmaNode { Id = "5:2", Name = "Top", Type = "TEXT", Characters = "Top", AbsoluteBoundingBox = new FigmaRectangle { X = 20, Y = 100, Width = 100, Height = 20 } },
                                    new FigmaNode { Id = "5:3", Name = "Middle", Type = "TEXT", Characters = "Middle", AbsoluteBoundingBox = new FigmaRectangle { X = 20, Y = 200, Width = 100, Height = 20 } }
                                ]
                            }
                        ]
                    }
                ]
            }
        };
    }
}
