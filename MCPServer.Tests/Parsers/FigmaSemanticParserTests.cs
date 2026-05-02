using FigmaMcpServer.Api.Models.FigmaApi;
using FigmaMcpServer.Api.Parsers;

namespace FigmaMcpServer.Tests.Parsers;

public sealed class FigmaSemanticParserTests
{
    [Fact]
    public void Parse_ExtractsSemanticModel_ForTopLevelFramesOnly()
    {
        var parser = new FigmaSemanticParser();
        var response = BuildSemanticSample();

        var result = parser.Parse(response);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, screen => screen.Screen == "Signup");
        Assert.Contains(result, screen => screen.Screen == "Dashboard");
        Assert.DoesNotContain(result, screen => screen.Screen == "Nested Inner Frame");
    }

    [Fact]
    public void Parse_NormalizesActionsAndExtractsFieldsAndLabels()
    {
        var parser = new FigmaSemanticParser();
        var response = BuildSemanticSample();

        var signup = Assert.Single(parser.Parse(response).Where(screen => screen.Screen == "Signup"));

        Assert.Contains("create_account", signup.Actions);
        Assert.Contains("login", signup.Actions);
        Assert.Contains("email", signup.Fields);
        Assert.Contains("password", signup.Fields);
        Assert.Contains("already_have_an_account", signup.Labels.Select(NormalizeForAssertion));
    }

    [Fact]
    public void Parse_FiltersStatusBarAndTimeNoise()
    {
        var parser = new FigmaSemanticParser();
        var response = BuildSemanticSample();

        var signup = Assert.Single(parser.Parse(response).Where(screen => screen.Screen == "Signup"));

        Assert.DoesNotContain(signup.Labels, label => label.Contains("9:41", StringComparison.Ordinal));
        Assert.DoesNotContain(signup.Labels, label => label.Contains("Battery", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Parse_DoesNotTreatInstructionTextAsActionOrField()
    {
        var parser = new FigmaSemanticParser();
        var response = BuildInstructionalContentSample();

        var screen = Assert.Single(parser.Parse(response));

        Assert.DoesNotContain("push_a_button_were_there_in_15_minutes_or_less", screen.Actions);
        Assert.DoesNotContain("enter_your_email_address_to_reset_your_password", screen.Fields);
        Assert.Contains(
            screen.Labels,
            label => label.Contains("Push a button", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Parse_FiltersKeypadAndStandaloneCurrencyNoise()
    {
        var parser = new FigmaSemanticParser();
        var response = BuildKeypadNoiseSample();

        var screen = Assert.Single(parser.Parse(response));

        Assert.DoesNotContain(screen.Labels, label => label is "0" or "9" or "ABC" or "WXYZ" or "₦");
        Assert.Contains("add_card", screen.Actions);
    }

    [Fact]
    public void Parse_WithFrameNodeId_ReturnsOnlyTargetScreen()
    {
        var parser = new FigmaSemanticParser();
        var response = BuildSemanticSample();

        var result = parser.Parse(response, "1:1");

        var screen = Assert.Single(result);
        Assert.Equal("Signup", screen.Screen);
    }

    [Fact]
    public void Parse_WithNestedTextNodeId_ResolvesParentScreen()
    {
        var parser = new FigmaSemanticParser();
        var response = BuildSemanticSample();

        var result = parser.Parse(response, "1-4");

        var screen = Assert.Single(result);
        Assert.Equal("Signup", screen.Screen);
    }

    [Fact]
    public void Parse_WithUnknownNodeId_ThrowsArgumentException()
    {
        var parser = new FigmaSemanticParser();
        var response = BuildSemanticSample();

        var action = () => parser.Parse(response, "999:999");

        Assert.Throws<ArgumentException>(action);
    }

    private static FigmaFileResponse BuildSemanticSample()
    {
        return new FigmaFileResponse
        {
            Name = "Delivery App",
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
                                Name = "Signup",
                                Type = "FRAME",
                                Children =
                                [
                                    new FigmaNode
                                    {
                                        Id = "1:2",
                                        Name = "Status Bar",
                                        Type = "FRAME",
                                        Children = [new FigmaNode { Id = "1:3", Name = "Time", Type = "TEXT", Characters = "9:41" }]
                                    },
                                    new FigmaNode { Id = "1:4", Name = "Email Input", Type = "TEXT", Characters = "Email" },
                                    new FigmaNode { Id = "1:5", Name = "Password Input", Type = "TEXT", Characters = "Password" },
                                    new FigmaNode { Id = "1:6", Name = "CTA", Type = "TEXT", Characters = "Create account" },
                                    new FigmaNode { Id = "1:7", Name = "Login", Type = "TEXT", Characters = "Login" },
                                    new FigmaNode { Id = "1:8", Name = "Already have an account?", Type = "TEXT", Characters = "Already have an account?" },
                                    new FigmaNode
                                    {
                                        Id = "1:9",
                                        Name = "Nested Inner Frame",
                                        Type = "FRAME",
                                        Children = [new FigmaNode { Id = "1:10", Name = "Inner Label", Type = "TEXT", Characters = "Inner Label" }]
                                    }
                                ]
                            },
                            new FigmaNode
                            {
                                Id = "2:1",
                                Name = "Dashboard",
                                Type = "FRAME",
                                Children =
                                [
                                    new FigmaNode { Id = "2:2", Name = "Welcome", Type = "TEXT", Characters = "Welcome Back" },
                                    new FigmaNode { Id = "2:3", Name = "search", Type = "TEXT", Characters = "Search" }
                                ]
                            }
                        ]
                    }
                ]
            }
        };
    }

    private static FigmaFileResponse BuildInstructionalContentSample()
    {
        return new FigmaFileResponse
        {
            Name = "Delivery App",
            Document = new FigmaNode
            {
                Id = "9:0",
                Name = "Document",
                Type = "DOCUMENT",
                Children =
                [
                    new FigmaNode
                    {
                        Id = "9:1",
                        Name = "Page 1",
                        Type = "CANVAS",
                        Children =
                        [
                            new FigmaNode
                            {
                                Id = "9:2",
                                Name = "Onboarding",
                                Type = "FRAME",
                                Children =
                                [
                                    new FigmaNode
                                    {
                                        Id = "9:3",
                                        Name = "Hero Copy",
                                        Type = "TEXT",
                                        Characters = "Push a button we're there in 15 minutes or less"
                                    },
                                    new FigmaNode
                                    {
                                        Id = "9:4",
                                        Name = "Forgot Password Helper",
                                        Type = "TEXT",
                                        Characters = "Enter your email address to reset your password"
                                    },
                                    new FigmaNode
                                    {
                                        Id = "9:5",
                                        Name = "Primary CTA",
                                        Type = "TEXT",
                                        Characters = "Login"
                                    }
                                ]
                            }
                        ]
                    }
                ]
            }
        };
    }

    private static FigmaFileResponse BuildKeypadNoiseSample()
    {
        return new FigmaFileResponse
        {
            Name = "Delivery App",
            Document = new FigmaNode
            {
                Id = "10:0",
                Name = "Document",
                Type = "DOCUMENT",
                Children =
                [
                    new FigmaNode
                    {
                        Id = "10:1",
                        Name = "Page 1",
                        Type = "CANVAS",
                        Children =
                        [
                            new FigmaNode
                            {
                                Id = "10:2",
                                Name = "Add card",
                                Type = "FRAME",
                                Children =
                                [
                                    new FigmaNode { Id = "10:3", Name = "Title", Type = "TEXT", Characters = "Add Card" },
                                    new FigmaNode { Id = "10:4", Name = "Currency", Type = "TEXT", Characters = "₦" },
                                    new FigmaNode
                                    {
                                        Id = "10:5",
                                        Name = "Keypad",
                                        Type = "FRAME",
                                        Children =
                                        [
                                            new FigmaNode { Id = "10:6", Name = "Key0", Type = "TEXT", Characters = "0" },
                                            new FigmaNode { Id = "10:7", Name = "Key9", Type = "TEXT", Characters = "9" },
                                            new FigmaNode { Id = "10:8", Name = "KeyABC", Type = "TEXT", Characters = "ABC" },
                                            new FigmaNode { Id = "10:9", Name = "KeyWXYZ", Type = "TEXT", Characters = "WXYZ" }
                                        ]
                                    }
                                ]
                            }
                        ]
                    }
                ]
            }
        };
    }

    private static string NormalizeForAssertion(string value)
    {
        return value.Replace(" ", "_", StringComparison.Ordinal)
            .Replace("?", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();
    }
}
