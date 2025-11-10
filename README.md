[![NuGet Version](https://img.shields.io/nuget/vpre/BinkyLabs.OpenApi.Overlays)](https://www.nuget.org/packages/BinkyLabs.OpenApi.Overlays) [![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/BinkyLabs/openapi-overlays-dotnet/dotnet.yml)](https://github.com/BinkyLabs/openapi-overlays-dotnet/actions/workflows/dotnet.yml)

# OpenAPI Overlay Library & CLI for dotnet

This project provides a .NET implementation of the [OpenAPI Overlay Specification](https://spec.openapis.org/overlay/latest.html), allowing you to dynamically apply overlays (patches) to existing OpenAPI documents (v3.0+), following the official OpenAPI Overlay 1.0.0 specification.

The library enables developers to programmatically apply overlays, validate them, and generate updated OpenAPI documents without relying on third-party tools like Swagger.

The CLI enables developers to apply overlays to an OpenAPI document from their favourite shell.

## CLI

### Installing the CLI

```shell
dotnet tool install -g BinkyLabs.OpenApi.Overlays.Tool --prerelease
```

### Usage

#### Apply an overlay to an OpenAPI description

The apply command applies the overlay actions to an OpenAPI description and preserves the source ordering of fields.

```shell
clio apply pathOrUrlToInputDescription --overlay pathOrUrlToOverlay -out pathForResultingDescription
```

> Note: the overlay argument can be specified multiple times, the order matters.

#### Apply and normalize

The apply command applies the overlay actions to an OpenAPI description and normalizes the description based on OpenAPI.net rules and fields ordering.

```shell
clio apply-and-normalize pathOrUrlToInputDescription --overlay pathOrUrlToOverlay -out pathForResultingDescription
```

> Note: the overlay argument can be specified multiple times, the order matters.

## Library

### Installing the library

You can install this library via the package explorer or using the following command.

```bash
dotnet add <pathToCsProj> package BinkyLabs.OpenApi.Overlays
```

### Examples

#### Parsing an Overlay document

The following example illustrates how you can load or parse an Overlay document from JSON or YAML.

```csharp
var (overlayDocument) = await OverlayDocument.LoadFromUrlAsync("https://source/overlay.json");
```

#### Applying an Overlay document to an OpenAPI document

The following example illustrates how you can apply an Overlay document to an OpenAPI document.

```csharp
var (resultOpenApiDocument) = await overlayDocument.ApplyToDocumentAndLoadAsync("https://source/openapi.json");
```

#### Applying multiple Overlay documents to an OpenAPI document

The following example illustrates how you can apply multiple Overlay documents to an OpenAPI document.

```csharp
var combinedOverlay = overlayDocument1.CombineWith(overlayDocument2);
// order matters during the combination, the actions will be appended
var (resultOpenApiDocument) = await combinedOverlay.ApplyToDocumentAndLoadAsync("https://source/openapi.json");
```

#### Serializing an Overlay document

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

## Experimental features

This library implements the following experimental features:

### Copy

The [copy proposal](https://github.com/OAI/Overlay-Specification/pull/150) to the Overlay specification works similarly to the update action, except it sources its value from another node. This library adds a property under an experimental flag, serializes and deserializes the value, and applies a copy overlay to an OpenAPI document.

```json
{
    "target": "$.info.title",
    "description": "Copy description to title",
    "x-copy": "$.info.description"
}
```

### Action Parameters

The [action parameters proposal](https://github.com/OAI/Overlay-Specification/pull/238) adds support for parameterized overlay actions. This allows you to define parameters that can be used for string interpolation and to generate multiple actions through matrix expansion.

#### String Interpolation

Parameters can be referenced in action properties using the `${parameterName}` syntax. The parameter name matches an environment variable name, with optional default values as fallback:

```json
{
    "target": "$.info.title",
    "description": "Update title with environment",
    "update": "API for ${environment}",
    "x-parameters": [
        {
            "name": "environment",
            "defaultValues": ["development", "staging", "production"]
        }
    ]
}
```

#### Matrix Expansion

When an action has parameters with multiple values, the action is expanded into multiple actions (one for each combination of parameter values):

```json
{
    "target": "$.paths./api/${version}/users.get.summary",
    "description": "Update summary for each version and environment",
    "update": "Get users - ${environment} (${version})",
    "x-parameters": [
        {
            "name": "version",
            "defaultValues": ["v1", "v2"]
        },
        {
            "name": "environment",
            "defaultValues": ["dev", "prod"]
        }
    ]
}
```

This single action expands to 4 actions (v1+dev, v1+prod, v2+dev, v2+prod).

#### Environment Variables

Parameters always try to read from environment variables first. The `name` property specifies the environment variable name:

```json
{
    "target": "$.servers[0].url",
    "description": "Set server URL from environment",
    "update": "https://${API_HOST}/api",
    "x-parameters": [
        {
            "name": "API_HOST"
        }
    ]
}
```

You can provide default values that are used when the environment variable is not set:

```json
{
    "target": "$.info.title",
    "description": "Update title with environment or default",
    "update": "API for ${environment}",
    "x-parameters": [
        {
            "name": "environment",
            "defaultValues": ["production"]
        }
    ]
}
```

You can also split environment variable values using a separator:

```json
{
    "target": "$.info.version",
    "description": "Update version for multiple environments",
    "update": "${version}",
    "x-parameters": [
        {
            "name": "VERSIONS",
            "separator": ","
        }
    ]
}
```

If the `VERSIONS` environment variable contains `"1.0.0,2.0.0,3.0.0"`, this will expand to 3 actions.

## Release notes

The OpenAPI Overlay Libraries releases notes are available from the [CHANGELOG](CHANGELOG.md)

## Debugging

## Contributing

This project welcomes contributions and suggestions.  Make sure you open an issue before sending any pull request to avoid any misunderstanding.

## Trademarks

