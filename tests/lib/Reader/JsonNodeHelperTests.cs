// Licensed under the MIT license.

using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Tests.Reader;

public class JsonNodeHelperTests
{
    private static ParsingContext Ctx() => new(new OverlayDiagnostic());

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

    [Fact]
    public void CheckMapNode_OnNonMap_Throws()
    {
        Assert.Throws<OverlayReaderException>(() => JsonNode.Parse("[]")!.CheckMapNode("foo", Ctx()));
    }

    [Fact]
    public void CheckMapNode_OnMap_ReturnsMap()
    {
        Assert.NotNull(JsonNode.Parse("{}")!.CheckMapNode("foo", Ctx()));
    }

    [Fact]
    public void CreateList_ProjectsObjects()
    {
        var result = JsonNode.Parse("""[{"a":1},{"b":2}]""")!.CreateList(static (n, _) => n, Ctx());
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void CreateSimpleList_ProjectsValues()
    {
        var result = JsonNode.Parse("""["a","b"]""")!.CreateSimpleList(static v => v.GetScalarValue(), Ctx());
        Assert.Equal(["a", "b"], result);
    }

    [Fact]
    public void CreateListOfAny_ReturnsItems()
    {
        var result = JsonNode.Parse("""[1,2,3]""")!.CreateListOfAny(Ctx());
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void CreateMap_BuildsDictionary()
    {
        var result = JsonNode.Parse("""{"a":{"x":1},"b":{"y":2}}""")!
            .CreateMap(static (n, _) => n.AsObject().GetReferencePointer() ?? "no-ref", Ctx());
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void CreateMap_NonObjectValueProducesDefault()
    {
        var result = JsonNode.Parse("""{"a": 1}""")!.CreateMap<object?>(static (n, _) => n, Ctx());
        Assert.Null(result["a"]);
    }

    [Fact]
    public void CreateSimpleMap_BuildsDictionary()
    {
        var result = JsonNode.Parse("""{"a":"x","b":"y"}""")!
            .CreateSimpleMap(static v => v.GetScalarValue(), Ctx());
        Assert.Equal("x", result["a"]);
        Assert.Equal("y", result["b"]);
    }

    [Fact]
    public void CreateSimpleMap_NonScalar_Throws()
    {
        Assert.Throws<OverlayReaderException>(() => JsonNode.Parse("""{"a":{}}""")!
            .CreateSimpleMap(static v => v.GetScalarValue(), Ctx()));
    }

    [Fact]
    public void CreateArrayMap_BuildsDictionary()
    {
        var result = JsonNode.Parse("""{"a":["x","y"],"b":["z"]}""")!
            .CreateArrayMap(static v => v.GetScalarValue(), Ctx());
        Assert.Equal(2, result["a"].Count);
        Assert.Single(result["b"]);
    }

    [Fact]
    public void CreateArrayMap_NonArray_Throws()
    {
        Assert.Throws<OverlayReaderException>(() => JsonNode.Parse("""{"a":"x"}""")!
            .CreateArrayMap(static v => v.GetScalarValue(), Ctx()));
    }

    [Fact]
    public void GetRaw_ReturnsJsonString()
    {
        Assert.Contains("\"a\"", JsonNode.Parse("""{"a":1}""")!.GetRaw());
    }

    [Fact]
    public void Reference_Identifier_Summary_Description_Found()
    {
        var map = JsonNode.Parse("""{ "$ref": "#/x", "$id": "id1", "summary": "s", "description": "d" }""")!.AsObject();

        Assert.Equal("#/x", map.GetReferencePointer());
        Assert.Equal("id1", map.GetJsonSchemaIdentifier());
        Assert.Equal("s", map.GetSummaryValue());
        Assert.Equal("d", map.GetDescriptionValue());
    }

    [Fact]
    public void Reference_Identifier_Summary_Description_NotFound()
    {
        var map = JsonNode.Parse("""{ "k": "v" }""")!.AsObject();

        Assert.Null(map.GetReferencePointer());
        Assert.Null(map.GetJsonSchemaIdentifier());
        Assert.Null(map.GetSummaryValue());
        Assert.Null(map.GetDescriptionValue());
    }

    [Fact]
    public void Reference_Identifier_Summary_Description_NullValue_ReturnsNull()
    {
        var map = JsonNode.Parse("""{ "$ref": null, "$id": null, "summary": null, "description": null }""")!.AsObject();

        Assert.Null(map.GetReferencePointer());
        Assert.Null(map.GetJsonSchemaIdentifier());
        Assert.Null(map.GetSummaryValue());
        Assert.Null(map.GetDescriptionValue());
    }

    [Fact]
    public void GetScalarValue_ByKey_ReturnsValue()
    {
        var map = JsonNode.Parse("""{"a":"hi"}""")!.AsObject();
        Assert.Equal("hi", map.GetScalarValue(JsonValue.Create("a")!, Ctx()));
    }

    [Fact]
    public void GetScalarValue_NonScalarKey_Throws()
    {
        var map = JsonNode.Parse("""{"a":{}}""")!.AsObject();
        Assert.Throws<OverlayReaderException>(() => map.GetScalarValue(JsonValue.Create("a")!, Ctx()));
    }

    [Fact]
    public void ParseMap_CallsFixedFieldMap()
    {
        var holder = new Holder();
        var fixedFields = new FixedFieldMap<Holder>
        {
            ["foo"] = (h, v, _) => h.Value = v.GetScalarValue()
        };

        JsonNode.Parse("""{"foo":"bar"}""")!.AsObject().ParseMap(holder, fixedFields, new(), Ctx());

        Assert.Equal("bar", holder.Value);
    }

    [Fact]
    public void ParseMap_CallsPatternFieldMap()
    {
        var holder = new Holder();
        var patternFields = new PatternFieldMap<Holder>
        {
            [k => k.StartsWith("x-")] = (h, k, v, _) => h.Value = $"{k}={v.GetScalarValue()}"
        };

        JsonNode.Parse("""{"x-foo":"bar"}""")!.AsObject().ParseMap(holder, new(), patternFields, Ctx());

        Assert.Equal("x-foo=bar", holder.Value);
    }

    [Fact]
    public void ParseMap_UnknownField_AddsDiagnostic()
    {
        var ctx = Ctx();
        JsonNode.Parse("""{"unknown":"bar"}""")!.AsObject().ParseMap(new Holder(), new(), new(), ctx);

        Assert.Contains(ctx.Diagnostic.Errors, e => e.Message.Contains("unknown is not a valid property"));
    }

    [Fact]
    public void ParseMap_SchemaField_IsIgnored()
    {
        var ctx = Ctx();
        JsonNode.Parse("""{"$schema":"https://example.com/schema"}""")!.AsObject().ParseMap(new Holder(), new(), new(), ctx);

        Assert.Empty(ctx.Diagnostic.Errors);
    }

    [Fact]
    public void ParseMap_NullProperty_IsIgnored()
    {
        var holder = new Holder();
        var fixedFields = new FixedFieldMap<Holder>
        {
            ["foo"] = (h, _, _) => h.Value = "called"
        };

        JsonNode.Parse("""{"foo":null}""")!.AsObject().ParseMap(holder, fixedFields, new(), Ctx());

        Assert.Null(holder.Value);
    }

    [Fact]
    public void ParseMap_OverlayReaderException_AddsDiagnostic()
    {
        var ctx = Ctx();
        var fixedFields = new FixedFieldMap<Holder>
        {
            ["foo"] = (_, _, _) => throw new OverlayReaderException("reader-err")
        };

        JsonNode.Parse("""{"foo":"bar"}""")!.AsObject().ParseMap(new Holder(), fixedFields, new(), ctx);

        Assert.Contains(ctx.Diagnostic.Errors, e => e.Message.Contains("reader-err"));
    }

    [Fact]
    public void ParseMap_OpenApiException_AddsDiagnostic()
    {
        var ctx = Ctx();
        var fixedFields = new FixedFieldMap<Holder>
        {
            ["foo"] = (_, _, _) => throw new OpenApiException("openapi-err")
        };

        JsonNode.Parse("""{"foo":"bar"}""")!.AsObject().ParseMap(new Holder(), fixedFields, new(), ctx);

        Assert.Contains(ctx.Diagnostic.Errors, e => e.Message.Contains("openapi-err"));
    }

    [Fact]
    public void ParseMap_PatternThrowsOverlayReaderException_AddsDiagnostic()
    {
        var ctx = Ctx();
        var patternFields = new PatternFieldMap<Holder>
        {
            [k => k.StartsWith("x-")] = (_, _, _, _) => throw new OverlayReaderException("pattern-err")
        };

        JsonNode.Parse("""{"x-foo":"bar"}""")!.AsObject().ParseMap(new Holder(), new(), patternFields, ctx);

        Assert.Contains(ctx.Diagnostic.Errors, e => e.Message.Contains("pattern-err"));
    }

    [Fact]
    public void ParseMap_PatternThrowsOpenApiException_AddsDiagnostic()
    {
        var ctx = Ctx();
        var patternFields = new PatternFieldMap<Holder>
        {
            [k => k.StartsWith("x-")] = (_, _, _, _) => throw new OpenApiException("pattern-openapi-err")
        };

        JsonNode.Parse("""{"x-foo":"bar"}""")!.AsObject().ParseMap(new Holder(), new(), patternFields, ctx);

        Assert.Contains(ctx.Diagnostic.Errors, e => e.Message.Contains("pattern-openapi-err"));
    }

    private class Holder
    {
        public string? Value { get; set; }
    }
}