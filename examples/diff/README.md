# Overlay Diff Generation Example

This directory contains examples of using the overlay generator to create overlay documents from two versions of an OpenAPI specification.

## Files

- `source-v1.json` - Version 1.0.0 of a Pet Store API
- `target-v2.json` - Version 2.0.0 of the same API with several changes

## Changes from V1 to V2

The following changes were made between v1 and v2:

1. **Info Object Updates:**
   - Version updated from `1.0.0` to `2.0.0`
   - Description enhanced
   - Added `contact` information

2. **Server Changes:**
   - URL updated from `/v1` to `/v2`
   - Added server description

3. **GET /pets Endpoint:**
   - Summary updated to mention pagination
   - Added new `offset` parameter
   - Added `default` values to parameters
   - Enhanced parameter descriptions
   - Added `400` error response

4. **POST /pets Endpoint:**
   - Completely new endpoint for creating pets

5. **Schema Changes:**
   - Pet schema: Added `status` field with enum values
   - Pet schema: Added validation constraints (`minLength`, `maxLength`) to `name`
   - Added new `NewPet` schema

## Using the Generator

### Programmatically

```csharp
using BinkyLabs.OpenApi.Overlays;
using BinkyLabs.OpenApi.Overlays.Generation;

// Generate overlay from file paths
var overlay = await OverlayGenerator.GenerateAsync(
    "examples/diff/source-v1.json",
    "examples/diff/target-v2.json",
    format: "json",
    info: new OverlayInfo
    {
        Title = "Pet Store API v1 to v2 Migration",
        Version = "1.0.0",
        Description = "Overlay to migrate from v1 to v2"
    });

// Serialize the overlay
using var outputStream = File.Create("migration-overlay.json");
await overlay.SerializeAsync(outputStream, OverlaySpecVersion.Overlay1_1, "json");
```

### From Streams

```csharp
using var sourceStream = File.OpenRead("source-v1.json");
using var targetStream = File.OpenRead("target-v2.json");

var overlay = await OverlayGenerator.GenerateAsync(
    sourceStream,
    targetStream,
    format: "json");
```

### From JsonNode

```csharp
using System.Text.Json.Nodes;

var sourceNode = JsonNode.Parse(await File.ReadAllTextAsync("source-v1.json"));
var targetNode = JsonNode.Parse(await File.ReadAllTextAsync("target-v2.json"));

var overlay = OverlayGenerator.Generate(sourceNode, targetNode);
```

## Applying the Generated Overlay

Once you have generated an overlay, you can apply it back to the source document:

```csharp
// Apply the overlay to transform v1 to v2
var result = await overlay.ApplyToDocumentAsync("source-v1.json");

// Verify the result matches v2
Assert.True(result.IsSuccessful);
```

## Use Cases

1. **API Versioning**: Track changes between API versions
2. **Documentation**: Document what changed between versions
3. **Migration**: Apply changes to customized versions of the API
4. **Change Review**: Review and approve API changes before deployment
5. **Regression Testing**: Ensure changes are applied correctly
