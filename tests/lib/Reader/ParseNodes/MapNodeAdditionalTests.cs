// Licensed under the MIT license.

using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;

namespace BinkyLabs.OpenApi.Overlays.Tests.Reader.ParseNodes;

public class MapNodeAdditionalTests
{
    private static MapNode MakeMap(string json)
    {
        var node = JsonNode.Parse(json)!;
        return new MapNode(new ParsingContext(new OverlayDiagnostic()), node);
    }

    [Fact]
    public void CreateMap_NonObjectValue_UsesDefault()
    {
        var map = MakeMap("""{ "a": "scalar", "b": { "k": "v" } }""");
        var result = map.CreateMap(static n => n.GetRaw());

        Assert.Equal(2, result.Count);
        Assert.Null(result["a"]);
        Assert.NotNull(result["b"]);
    }

    [Fact]
    public void CreateSimpleMap_NonScalarValue_Throws()
    {
        var map = MakeMap("""{ "a": { "nested": true } }""");
        Assert.Throws<OverlayReaderException>(() => map.CreateSimpleMap(static v => v.GetScalarValue()));
    }

    [Fact]
    public void CreateArrayMap_NonArrayValue_Throws()
    {
        var map = MakeMap("""{ "a": "not-array" }""");
        Assert.Throws<OverlayReaderException>(() => map.CreateArrayMap(static v => v.GetScalarValue()));
    }

    [Fact]
    public void CreateArrayMap_ArrayValue_Succeeds()
    {
        var map = MakeMap("""{ "a": ["x", "y"] }""");
        var result = map.CreateArrayMap(static v => v.GetScalarValue());

        Assert.Single(result);
        Assert.Equal(2, result["a"].Count);
    }

    [Fact]
    public void GetScalarValue_NonScalarChild_Throws()
    {
        var map = MakeMap("""{ "a": { "nested": true } }""");
        var key = new ValueNode(new ParsingContext(new OverlayDiagnostic()), JsonValue.Create("a")!);
        Assert.Throws<OverlayReaderException>(() => map.GetScalarValue(key));
    }

    [Fact]
    public void Reference_Identifier_Summary_Description_Found()
    {
        var map = MakeMap("""{ "$ref": "#/x", "$id": "id1", "summary": "s", "description": "d" }""");

        Assert.Equal("#/x", map.GetReferencePointer());
        Assert.Equal("id1", map.GetJsonSchemaIdentifier());
        Assert.Equal("s", map.GetSummaryValue());
        Assert.Equal("d", map.GetDescriptionValue());
    }

    [Fact]
    public void Reference_Identifier_Summary_Description_NotFound()
    {
        var map = MakeMap("""{ "k": "v" }""");

        Assert.Null(map.GetReferencePointer());
        Assert.Null(map.GetJsonSchemaIdentifier());
        Assert.Null(map.GetSummaryValue());
        Assert.Null(map.GetDescriptionValue());
    }

    [Fact]
    public void Indexer_MissingKey_ReturnsNull()
    {
        var map = MakeMap("""{ "a": "v" }""");
        Assert.Null(map["missing"]);
    }

    [Fact]
    public void Indexer_ExistingKey_ReturnsPropertyNode()
    {
        var map = MakeMap("""{ "a": "v" }""");
        Assert.NotNull(map["a"]);
    }

    [Fact]
    public void Constructor_NonObject_Throws()
    {
        var ctx = new ParsingContext(new OverlayDiagnostic());
        var node = JsonNode.Parse("\"scalar\"")!;
        Assert.Throws<OverlayReaderException>(() => new MapNode(ctx, node));
    }
}