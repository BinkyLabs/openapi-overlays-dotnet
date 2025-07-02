using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class OverlayDocumentTests
{
    [Fact]
    public void SerializeAsV1_ShouldWriteCorrectJson()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Overlay = "1.0.0",
            Info = new OverlayInfo
            {
                Title = "Test Overlay",
                Version = "1.0.0"
            },
            Extends = "x-extends",
            Actions = new List<OverlayAction>
            {
                new OverlayAction
                {
                    Target = "Test Target",
                    Description = "Test Description",
                    Remove = true
                }
            }


        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson = """
        {
            "overlay": "1.0.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "extends": "x-extends",
            "actions": [
                {
                    "target": "Test Target",
                    "description": "Test Description",
                    "remove": true
                }
            ]
        }
        """;

        // Act
        overlayDocument.SerializeAsV1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);


        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void Deserialize_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "overlay": "1.0.0",
            "info": {
                "title": "Test Overlay",
                "version": "2.0.0"
            },
            "extends": "x-extends",
            "actions": [
                {
                    "target": "Test Target",
                    "description": "Test Description",
                    "remove": true
                },{
                    "target": "Test Target 2",
                    "description": "Test Description 2",
                    "remove": false
                }
            ]
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var overlayDocument = OverlayV1Deserializer.LoadDocument(parseNode, new OverlayDocument());

        // Assert
        Assert.NotNull(overlayDocument);
        Assert.Equal("1.0.0", overlayDocument.Overlay);
        Assert.Equal("Test Overlay", overlayDocument.Info?.Title);
        Assert.Equal("2.0.0", overlayDocument.Info?.Version);
        Assert.Equal("x-extends", overlayDocument.Extends);

        // Assert the 2 action
        Assert.NotNull(overlayDocument.Actions);
        Assert.Equal(2, overlayDocument.Actions.Count);
        Assert.Equal("Test Target", overlayDocument.Actions[0].Target);
        Assert.Equal("Test Description", overlayDocument.Actions[0].Description);
        Assert.True(overlayDocument.Actions[0].Remove);
        Assert.Equal("Test Target 2", overlayDocument.Actions[1].Target);
        Assert.Equal("Test Description 2", overlayDocument.Actions[1].Description);
        Assert.False(overlayDocument.Actions[1].Remove);
    }
}