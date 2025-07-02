# OpenAPI Overlay Libraries for dotnet

This project provides a .NET implementation of the [OpenAPI Overlay Specification](https://spec.openapis.org/overlay/latest.html), allowing you to dynamically apply overlays (patches) to existing OpenAPI documents (v3.0+), following the official OpenAPI Overlay 1.0.0 specification.

The library enables developers to programmatically apply overlays, validate them, and generate updated OpenAPI documents without relying on third-party tools like Swagger.


## Build Status


## Libraries

| Library                                                              | NuGet Release                                                                                                                                                                              |
|----------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|

## Examples

### Serializing an Overlay document

The following example illustrates how you can serialize an Overlay document, built by the application or previously parsed, to JSON.

```csharp
var overlayDocument = new OverlayDocument
{
    Info = new OverlayInfo
    {
        Title = "Test Overlay",
        Version = "1.0.0"
    },
    Extends = "foo/myDescription.json",
    Actions = new List<OverlayAction>
    {
        new OverlayAction
        {
            Target = "$.paths['/bar']",
            Description = "Updates bar path item",
            Remove = true
        }
    }
};

using var textWriter = new StringWriter();
var writer = new OpenApiJsonWriter(textWriter);
using var textWriter = new StringWriter();
var writer = new OpenApiJsonWriter(textWriter);
var jsonResult = textWriter.ToString();
// or use flush async if the underlying writer is a stream writer to a file or network stream
```

## Release notes

The OpenAPI Overlay Libraries releases notes are available from the [CHANGELOG](CHANGELOG.md)

## Debugging


## Contributing

This project welcomes contributions and suggestions.  Make sure you open an issue before sending any pull request to avoid any misunderstanding.

## Trademarks

