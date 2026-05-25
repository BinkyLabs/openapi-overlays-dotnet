// Licensed under the MIT license.

using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Tests.Reader;

public class JsonNodeHelperTests
{
    [Fact]
    public void GetScalarValue_OnJsonValue_ReturnsString()
    {
        JsonNode node = JsonValue.Create("hello")!;
        Assert.Equal("hello", node.GetScalarValue());
    }

    [Fact]
    public void GetScalarValue_OnJsonValueNumber_ReturnsString()
    {
        JsonNode node = JsonValue.Create(42)!;
        Assert.Equal("42", node.GetScalarValue());
    }

    [Fact]
    public void GetScalarValue_OnJsonObject_Throws()
    {
        JsonNode node = new JsonObject();
        Assert.Throws<OpenApiException>(() => node.GetScalarValue());
    }
}