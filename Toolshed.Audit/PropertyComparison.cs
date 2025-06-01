using System;

namespace Toolshed.Audit;

public readonly struct PropertyComparison
{
    public string Name { get; init; }
    public string OldValue { get; init; }
    public string NewValue { get; init; }
    public string Type { get; init; }

    public PropertyComparison(string name, object? oldValue, object? newValue)
    {
        Name = name;
        string type = string.Empty;
        string oldVal = string.Empty;
        string newVal = string.Empty;

        if (oldValue != null)
        {
            type = oldValue.GetType().Name;
            oldVal = oldValue.ToString() ?? string.Empty;
        }
        if (newValue != null)
        {
            if (string.IsNullOrEmpty(type))
                type = newValue.GetType().Name;
            newVal = newValue.ToString() ?? string.Empty;
        }
        Type = type;
        OldValue = oldVal;
        NewValue = newVal;
    }
}
