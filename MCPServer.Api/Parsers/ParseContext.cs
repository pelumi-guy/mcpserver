using FigmaMcpServer.Api.DTOs;

namespace FigmaMcpServer.Api.Parsers;

public sealed class ParseContext
{
    private readonly HashSet<string> _actionSet = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _labelSet = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _fieldSet = new(StringComparer.OrdinalIgnoreCase);

    public ParseContext(ScreenDto screen)
    {
        Screen = screen;
    }

    public ScreenDto Screen { get; }

    public void AddAction(string action)
    {
        if (string.IsNullOrWhiteSpace(action) || !_actionSet.Add(action))
        {
            return;
        }

        Screen.Actions.Add(action);
    }

    public void AddLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label) || !_labelSet.Add(label))
        {
            return;
        }

        Screen.Labels.Add(label);
    }

    public void AddField(FieldDto field)
    {
        var key = $"{field.Name}:{field.Type}";
        if (string.IsNullOrWhiteSpace(field.Name) || !_fieldSet.Add(key))
        {
            return;
        }

        Screen.Fields.Add(field);
    }
}
