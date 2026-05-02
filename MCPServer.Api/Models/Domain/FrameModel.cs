namespace FigmaMcpServer.Api.Models.Domain;

public sealed record FrameModel(
    string Name,
    IReadOnlyList<InputFieldModel> Fields,
    IReadOnlyList<ButtonModel> Actions,
    IReadOnlyList<TextModel> Labels);
