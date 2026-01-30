# Overlay Diff Generation

The OpenAPI Overlays library includes a powerful diff generation feature that can automatically create overlay documents by comparing two OpenAPI specifications. This is useful for tracking changes between API versions, generating migration guides, and applying consistent changes across multiple API variants.

## Overview

The `OverlayGenerator` class analyzes two OpenAPI documents (source and target) and generates an `OverlayDocument` containing a series of actions that transform the source into the target. The generator intelligently handles:

- Property additions
- Property removals
- Property modifications
- Nested object changes
- Array updates

## Basic Usage

### Generate from JsonNode

```csharp
using System.Text.Json.Nodes;
using BinkyLabs.OpenApi.Overlays.Generation;

var source = JsonNode.Parse(await File.ReadAllTextAsync("api-v1.json"));
var target = JsonNode.Parse(await File.ReadAllTextAsync("api-v2.json"));

var overlay = OverlayGenerator.Generate(source, target, info: new OverlayInfo
{
    Title = "API v1 to v2 Migration",
    Version = "1.0.0",
    Description = "Automated migration from v1 to v2"
});
```

### Generate from File Paths

```csharp
var overlay = await OverlayGenerator.GenerateAsync(
    sourcePath: "api-v1.json",
    targetPath: "api-v2.json",
    format: "json"
);
```

### Generate from Streams

```csharp
using var sourceStream = File.OpenRead("api-v1.yaml");
using var targetStream = File.OpenRead("api-v2.yaml");

var overlay = await OverlayGenerator.GenerateAsync(
    sourceStream,
    targetStream,
    format: "yaml",
    info: new OverlayInfo
    {
        Title = "YAML API Migration",
        Version = "1.0.0"
    }
);
```

### Generate from URLs

```csharp
var overlay = await OverlayGenerator.GenerateAsync(
    sourcePath: "https://api.example.com/openapi-v1.json",
    targetPath: "https://api.example.com/openapi-v2.json",
    format: "json"
);
```

## Generated Actions

The generator creates different types of overlay actions based on the changes detected:

### Property Additions

When a property exists in the target but not in the source:

```json
{
  "target": "$.info",
  "description": "Add property 'contact'",
  "update": {
    "contact": {
      "name": "API Support",
      "email": "support@example.com"
    }
  }
}
```

### Property Removals

When a property exists in the source but not in the target:

```json
{
  "target": "$.info.deprecated",
  "description": "Remove property 'deprecated'",
  "remove": true
}
```

### Property Modifications

When a property value changes:

```json
{
  "target": "$.info.version",
  "description": "Update property 'version'",
  "update": "2.0.0"
}
```

### Nested Object Changes

The generator recursively processes nested objects:

```json
{
  "target": "$.servers[0].url",
  "description": "Update property 'url'",
  "update": "https://api.example.com/v2"
}
```

### Array Updates

Arrays are replaced entirely when they differ:

```json
{
  "target": "$.info.tags",
  "description": "Update array at 'tags'",
  "update": ["pets", "store", "user"]
}
```

## Serializing Generated Overlays

Once generated, you can serialize the overlay to JSON or YAML:

### JSON

```csharp
using Microsoft.OpenApi;
using var outputStream = File.Create("migration-overlay.json");
await overlay.SerializeAsync(outputStream, OverlaySpecVersion.Overlay1_1, OpenApiConstants.Json);
```

### YAML

```csharp
using Microsoft.OpenApi;
using var outputStream = File.Create("migration-overlay.yaml");
await overlay.SerializeAsync(outputStream, OverlaySpecVersion.Overlay1_1, OpenApiConstants.Yaml);
```

## Applying Generated Overlays

After generating an overlay, you can apply it back to verify correctness:

```csharp
// Generate the overlay
var overlay = await OverlayGenerator.GenerateAsync("v1.json", "v2.json");

// Apply it to the source
var result = await overlay.ApplyToDocumentAsync("v1.json");

// Verify the result
Assert.True(result.IsSuccessful);
Assert.Empty(result.Diagnostic.Errors);

// The result document should match the target
var resultJson = JsonSerializer.Serialize(result.Document);
var targetJson = await File.ReadAllTextAsync("v2.json");
Assert.Equal(targetJson, resultJson);
```

## JSONPath Generation

The generator uses the `JsonPathBuilder` helper to create properly formatted JSONPath expressions. The builder handles:

- Simple property names: `$.info.title`
- Properties with special characters: `$.paths['/users']`
- Properties with spaces: `$['my property']`
- Properties with dashes: `$['x-custom']`
- Array indices: `$.servers[0]`
- Single quote escaping: `$['it\'s']`

## Use Cases

### 1. API Versioning

Track changes between API versions automatically:

```csharp
var v1ToV2 = await OverlayGenerator.GenerateAsync("api-v1.json", "api-v2.json");
var v2ToV3 = await OverlayGenerator.GenerateAsync("api-v2.json", "api-v3.json");

// Store overlays for historical reference
await SaveOverlay(v1ToV2, "migrations/v1-to-v2.json");
await SaveOverlay(v2ToV3, "migrations/v2-to-v3.json");
```

### 2. Multi-tenant API Customization

Generate overlays for customer-specific customizations:

```csharp
var baseApi = await LoadOpenApi("base-api.json");
var customerApi = await LoadOpenApi("customer-a-api.json");

var customizationOverlay = OverlayGenerator.Generate(baseApi, customerApi, new OverlayInfo
{
    Title = "Customer A Customizations",
    Version = "1.0.0"
});

// Apply customizations to the base API for other customers
var customerBApi = await customizationOverlay.ApplyToDocumentAsync("base-api.json");
```

### 3. Change Review Process

Generate overlays for code review:

```csharp
// Developer proposes API changes
var proposedChanges = await OverlayGenerator.GenerateAsync(
    "current-api.json",
    "proposed-api.json"
);

// Review team examines the overlay
Console.WriteLine($"Total changes: {proposedChanges.Actions.Count}");
foreach (var action in proposedChanges.Actions)
{
    Console.WriteLine($"  - {action.Description} at {action.Target}");
}

// Approve and apply
if (approved)
{
    var result = await proposedChanges.ApplyToDocumentAndLoadAsync("current-api.json");
    await SaveOpenApi(result.Document, "current-api.json");
}
```

### 4. Documentation Generation

Create human-readable change documentation:

```csharp
var overlay = await OverlayGenerator.GenerateAsync("v1.json", "v2.json");

var markdown = new StringBuilder();
markdown.AppendLine("# API Changes from v1 to v2");
markdown.AppendLine();

foreach (var action in overlay.Actions)
{
    if (action.Remove == true)
    {
        markdown.AppendLine($"- **Removed**: `{action.Target}`");
    }
    else if (action.Update != null)
    {
        markdown.AppendLine($"- **Modified**: `{action.Target}`");
        markdown.AppendLine($"  - {action.Description}");
    }
}

await File.WriteAllTextAsync("CHANGELOG.md", markdown.ToString());
```

### 5. Regression Testing

Ensure API changes are consistent across environments:

```csharp
// Generate overlay from dev to prod
var expectedChanges = await OverlayGenerator.GenerateAsync(
    "dev-api.json",
    "prod-api.json"
);

// Verify staging matches the expected changes
var stagingResult = await expectedChanges.ApplyToDocumentAsync("dev-api.json");
var stagingActual = await LoadOpenApi("staging-api.json");

Assert.True(JsonNode.DeepEquals(stagingResult.Document, stagingActual));
```

## Advanced Configuration

### Custom Reader Settings

```csharp
var readerSettings = new OverlayReaderSettings
{
    OpenApiSettings = new OpenApiReaderSettings
    {
        BaseUrl = new Uri("https://api.example.com")
    }
};

var overlay = await OverlayGenerator.GenerateAsync(
    "api-v1.json",
    "api-v2.json",
    readerSettings: readerSettings
);
```

### Custom Info Metadata

```csharp
var overlay = OverlayGenerator.Generate(
    sourceNode,
    targetNode,
    info: new OverlayInfo
    {
        Title = "Quarterly API Update",
        Version = "2024.Q1",
        Description = "Automated changes for Q1 2024 release",
        Extensions = new Dictionary<string, IOverlayExtension>
        {
            ["x-generated-at"] = new JsonNodeExtension(JsonValue.Create(DateTime.UtcNow)),
            ["x-generator"] = new JsonNodeExtension(JsonValue.Create("OverlayGenerator"))
        }
    }
);
```

## Limitations and Considerations

1. **Array Comparison**: Arrays are compared by full replacement. Individual array item changes are not tracked granularly.

2. **Complex Restructuring**: The generator works best for incremental changes. Major structural rewrites may produce many actions.

3. **Circular References**: Ensure your OpenAPI documents don't contain circular references, as the generator recurses through the structure.

4. **Large Documents**: For very large API specifications, consider generating overlays for specific sections rather than the entire document.

5. **Semantic Understanding**: The generator performs structural comparison only. It doesn't understand semantic meaning (e.g., a renamed property is seen as a removal + addition).

## Best Practices

1. **Version Control**: Store generated overlays in version control alongside your API specifications.

2. **Naming Conventions**: Use descriptive names for generated overlays: `v1.0-to-v1.1-overlay.json`

3. **Documentation**: Include the `description` field in the `info` object to document the purpose of the overlay.

4. **Testing**: Always test generated overlays by applying them back to the source document to verify correctness.

5. **Incremental Changes**: Generate overlays between consecutive versions rather than skipping versions.

6. **Review Actions**: Review generated actions before applying them in production to catch unexpected changes.

## See Also

- [Overlay Specification](https://spec.openapis.org/overlay/v1.0.0)
- [Applying Overlays](./APPLYING.md)
- [Examples](../examples/diff/)
