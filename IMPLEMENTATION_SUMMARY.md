# Overlay Diff Generation Implementation Summary

## Overview

Implemented a complete diff generation feature that automatically creates overlay documents by comparing two OpenAPI specifications. This feature enables users to track API changes, generate migration guides, and apply consistent changes across multiple API variants.

## Implementation Details

### Core Components

1. **OverlayGenerator** (`src/lib/Generation/OverlayGenerator.cs`)
   - Static class providing diff generation functionality
   - Compares two OpenAPI documents (source and target) as JsonNode objects
   - Generates an OverlayDocument with actions to transform source into target
   - Supports loading from files, URIs, or streams
   - Handles JSON and YAML formats

2. **JsonPathBuilder** (`src/lib/Generation/JsonPathBuilder.cs`)
   - Helper class for building valid JSONPath expressions
   - Handles special characters in property names
   - Escapes properties with spaces, slashes, dashes, etc.
   - Supports array indexing
   - Properly formats bracket notation

### API Surface

#### Generate Methods

```csharp
// From JsonNode objects
OverlayDocument Generate(JsonNode sourceDocument, JsonNode targetDocument)
OverlayDocument Generate(JsonNode sourceDocument, JsonNode targetDocument, OverlayInfo? info)

// From file paths or URIs
Task<OverlayDocument> GenerateAsync(string sourcePath, string targetPath)
Task<OverlayDocument> GenerateAsync(string sourcePath, string targetPath, CancellationToken cancellationToken)
Task<OverlayDocument> GenerateAsync(string sourcePath, string targetPath, string? format, OverlayInfo? info, OverlayReaderSettings? readerSettings, CancellationToken cancellationToken)

// From streams
Task<OverlayDocument> GenerateFromStreamsAsync(Stream sourceStream, Stream targetStream)
Task<OverlayDocument> GenerateFromStreamsAsync(Stream sourceStream, Stream targetStream, CancellationToken cancellationToken)
Task<OverlayDocument> GenerateFromStreamsAsync(Stream sourceStream, Stream targetStream, string? format, OverlayInfo? info, OverlayReaderSettings? readerSettings, CancellationToken cancellationToken)
```

### Diff Algorithm

The generator recursively compares JSON structures and creates overlay actions for:

1. **Property Removals**: When a property exists in source but not in target
   - Generates `remove: true` action

2. **Property Additions**: When a property exists in target but not in source
   - Generates `update` action with the new property

3. **Property Modifications**: When a property value differs
   - For nested objects: Recurses into the object
   - For arrays: Generates replacement action
   - For simple values: Generates update action

4. **Null Handling**: Properly handles null values in both directions

### Test Coverage

#### JsonPathBuilder Tests (`tests/lib/Generation/JsonPathBuilderTests.cs`)
- Simple property paths
- Nested property paths
- Properties with special characters (slashes, dashes)
- Array indices
- Bracket notation escaping

#### OverlayGenerator Tests (`tests/lib/Generation/OverlayGeneratorTests.cs`)
- Simple property changes
- Property removals
- Property additions
- Identical documents (no changes)
- File-based generation

### Documentation

1. **User Guide** (`docs/DIFF_GENERATION.md`)
   - Comprehensive guide on using the diff generation feature
   - Multiple usage examples
   - Use cases: versioning, multi-tenant customization, change review, documentation
   - Best practices and limitations

2. **Example Files** (`examples/diff/`)
   - `source-v1.json`: Pet Store API v1.0.0
   - `target-v2.json`: Pet Store API v2.0.0 with various changes
   - `README.md`: Documentation of changes and usage examples

### Public API Changes

Added to `src/lib/PublicAPI.Unshipped.txt`:
- `BinkyLabs.OpenApi.Overlays.Generation.OverlayGenerator` class
- All public methods for generation

## Usage Examples

### Basic Usage

```csharp
using BinkyLabs.OpenApi.Overlays.Generation;

// From JsonNode
var source = JsonNode.Parse(await File.ReadAllTextAsync("api-v1.json"));
var target = JsonNode.Parse(await File.ReadAllTextAsync("api-v2.json"));
var overlay = OverlayGenerator.Generate(source, target);

// From file paths
var overlay = await OverlayGenerator.GenerateAsync("api-v1.json", "api-v2.json");

// With custom info
var overlay = await OverlayGenerator.GenerateAsync(
    "api-v1.json", 
    "api-v2.json",
    format: "json",
    info: new OverlayInfo { Title = "Migration Overlay", Version = "1.0.0" },
    readerSettings: null,
    cancellationToken: CancellationToken.None);
```

### Applying Generated Overlays

```csharp
// Generate the overlay
var overlay = await OverlayGenerator.GenerateAsync("v1.json", "v2.json");

// Apply it back to verify
var result = await overlay.ApplyToDocumentAsync("v1.json");
Assert.True(result.IsSuccessful);
```

## Features

? Compares two OpenAPI documents and generates diff overlay
? Supports JSON and YAML formats
? Handles nested objects recursively
? Detects property additions, removals, and modifications
? Properly escapes JSONPath special characters
? Comprehensive unit tests
? Complete documentation with examples
? Complex sample files (Pet Store API v1 to v2)

## Design Decisions

1. **Static Class**: Made `OverlayGenerator` static since it doesn't maintain state
2. **No Optional Parameters**: Avoided optional parameters in public API to comply with API analyzer rules
3. **Separate Overloads**: Provided clear overloads for different parameter combinations
4. **Recursive Algorithm**: Uses recursion to handle nested objects naturally
5. **Array Replacement**: Arrays are replaced entirely rather than element-by-element comparison
6. **JSONPath Escaping**: Proper handling of special characters in property names

## Testing

All tests pass (10/10):
- 5 JsonPathBuilder tests
- 5 OverlayGenerator tests

Build succeeded with no errors or warnings.

## Future Enhancements

Potential improvements for future versions:
1. Granular array diff (element-by-element comparison)
2. Semantic understanding (detect renames vs remove+add)
3. Configurable diff strategies
4. Performance optimizations for large documents
5. Diff visualization/reporting tools
6. CLI command for diff generation

## Commit Message

```
feat(generation): add overlay diff generation feature

Implement automatic overlay document generation by comparing two OpenAPI
specifications. The OverlayGenerator class can create overlays from JsonNode
objects, file paths, URIs, or streams, supporting both JSON and YAML formats.

Features:
- Generate overlay actions by diffing source and target documents
- Detect property additions, removals, and modifications
- Handle nested objects and arrays
- Proper JSONPath escaping with JsonPathBuilder helper
- Comprehensive unit tests and documentation
- Example files showing Pet Store API v1 to v2 migration

Breaking changes: None
```
