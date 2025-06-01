using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using Toolshed.Audit;

namespace Toolshed.Audit.Tests;

public class PropertyComparisonTests
{
    [Fact]
    public void CanDeserializeListOfPropertyComparisonFromJson()
    {
        string json = "[" +
            "{ \"Name\": \"Prop1\", \"OldValue\": \"A\", \"NewValue\": \"B\", \"Type\": \"String\" }," +
            "{ \"Name\": \"Prop2\", \"OldValue\": \"1\", \"NewValue\": \"2\", \"Type\": \"Int32\" }," +
            "{ \"Name\": \"Prop3\", \"OldValue\": \"true\", \"NewValue\": \"false\", \"Type\": \"Boolean\" }," +
            "{ \"Name\": \"Prop4\", \"OldValue\": \"2023-01-01\", \"NewValue\": \"2024-01-01\", \"Type\": \"DateTime\" }," +
            "{ \"Name\": \"Prop5\", \"OldValue\": \"X\", \"NewValue\": \"Y\", \"Type\": \"Char\" }" +
        "]";

        var result = JsonSerializer.Deserialize<List<PropertyComparison>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        Assert.Equal("Prop1", result[0].Name);
        Assert.Equal("A", result[0].OldValue);
        Assert.Equal("B", result[0].NewValue);
        Assert.Equal("String", result[0].Type);
    }
}
