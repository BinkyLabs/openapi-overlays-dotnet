[![NuGet Version](https://img.shields.io/nuget/vpre/BinkyLabs.OpenApi.Overlays)](https://www.nuget.org/packages/BinkyLabs.OpenApi.Overlays) [![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/BinkyLabs/openapi-overlays-dotnet/dotnet.yml)](https://github.com/BinkyLabs/openapi-overlays-dotnet/actions/workflows/dotnet.yml)

# OpenAPI Overlay Library & CLI for dotnet

This project provides a .NET implementation of the [OpenAPI Overlay Specification](https://spec.openapis.org/overlay/latest.html), allowing you to dynamically apply overlays (patches) to existing OpenAPI documents (v3.0+), following the official OpenAPI Overlay 1.0.0 and 1.1.0 specification.

The library enables developers to programmatically apply overlays, validate them, and generate updated OpenAPI documents without relying on third-party tools like Swagger.

The CLI enables developers to apply overlays to an OpenAPI document from their favourite shell.

## CLI

### Installing the CLI

```shell
dotnet tool install -g BinkyLabs.OpenApi.Overlays.Tool
```

### Usage

#### Apply an overlay to an OpenAPI description

The apply command applies the overlay actions to an OpenAPI description and preserves the source ordering of fields.

```shell
clio apply pathOrUrlToInputDescription --overlay pathOrUrlToOverlay -out pathForResultingDescription
```

> Note: the overlay argument can be specified multiple times, the order matters.

##### Options

- `--overlay` or `-o`: Path to overlay file(s). Can be specified multiple times. (Required)
- `--output` or `-out`: Path for the output file. (Required)
- `--force` or `-f`: Overwrite output file without confirmation.
- `--strict` or `-s`: Treat targets that match zero nodes as errors instead of warnings. Useful in CI scenarios to ensure overlays stay in sync with the source description.

#### Apply and normalize

The apply command applies the overlay actions to an OpenAPI description and normalizes the description based on OpenAPI.net rules and fields ordering.

```shell
clio apply-and-normalize pathOrUrlToInputDescription --overlay pathOrUrlToOverlay -out pathForResultingDescription
```

> Note: the overlay argument can be specified multiple times, the order matters.

##### Options

- `--overlay` or `-o`: Path to overlay file(s). Can be specified multiple times. (Required)
- `--output` or `-out`: Path for the output file. (Required)
- `--force` or `-f`: Overwrite output file without confirmation.
- `--strict` or `-s`: Treat targets that match zero nodes as errors instead of warnings. Useful in CI scenarios to ensure overlays stay in sync with the source description.

## GitHub Action

Use the OpenAPI Overlays CLI as a GitHub Action in your workflows. For complete documentation, see [ACTION.md](ACTION.md).

```yaml
- name: Apply OpenAPI Overlays
  uses: BinkyLabs/openapi-overlays-dotnet@v2
  with:
    input: 'openapi.yaml'
    overlays: 'overlay.yaml'
    output: 'openapi-modified.yaml'
```

### Inputs

- `input` (required): Path to the input OpenAPI document (YAML or JSON)
- `overlays` (required): Paths to overlay file(s), separated by newlines or spaces. Multiple overlays will be applied in order.
- `output` (required): Path for the output file
- `command` (optional): Command to run - `apply` (default, preserves field ordering) or `apply-and-normalize` (normalizes with OpenAPI.net rules)
- `force` (optional): Overwrite output file without confirmation (default: `true`)

### Example with Multiple Overlays

```yaml
- name: Apply Multiple OpenAPI Overlays
  uses: BinkyLabs/openapi-overlays-dotnet@v2
  with:
    input: 'openapi.yaml'
    overlays: |
      overlay1.yaml
      overlay2.yaml
      overlay3.yaml
    output: 'openapi-modified.yaml'
    command: 'apply-and-normalize'
```

## Docker Quick Start

Run the CLI in a Docker container without installing .NET:
Docker images are available at `ghcr.io/binkylabs/openapi-overlays-dotnet:latest`

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
    Actions = new List<IOverlayAction>
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
document.SerializeAsV1(writer);
var jsonResult = textWriter.ToString();
// or use flush async if the underlying writer is a stream writer to a file or network stream
```

## Experimental features

This library implements the following experimental features:

### Reusable Actions (Preview)

> **Note**: Reusable Actions are an experimental, preview feature. When using this library with ReusableActions, you must suppress the **BOO002** diagnostic code in your build configuration, as this feature is not yet part of the official OpenAPI Overlay Specification.

Reusable Actions allow you to define action templates in the `components.actions` section that can be referenced and reused multiple times throughout your overlay. This reduces duplication and makes overlays more maintainable.

#### Simple Example - Local Overrides

This example shows how a reusable action can provide shared update content while a reference overrides the target locally:

**Source OpenAPI:**

```yaml
openapi: 3.2.0
info:
  title: Example API
  version: 1.0.0
paths:
  /items:
    get:
      responses:
        200:
          description: OK
  /some-items:
    delete:
      responses:
        200:
          description: OK
```

**Overlay:**

```yaml
overlay: 1.2.0
info:
  title: Use reusable actions to insert error responses
  version: 1.0.0
x-components:
  actions:
    errorResponse:
      fields:
        update:
          404:
            description: Not Found
            content:
              application/json:
                schema:
                  type: object
                  properties:
                    message:
                      type: string
        description: Adds an error response to the operation
actions:
  - x-$ref: '#/components/actions/errorResponse'
    # Override the target from the reusable action
    target: "$.paths['/items'].get.responses"
  - x-$ref: '#/components/actions/errorResponse'
    # Override the target from the reusable action
    target: "$.paths['/some-items'].delete.responses"
```

#### Complex Example - Parameters and Environment Variables

This example shows how a reusable action can use parameters and environment variables for dynamic string interpolation:

**Source OpenAPI:**

```yaml
openapi: 3.2.0
info:
  title: Example API
  version: 1.0.0
paths:
  /items:
    get:
      responses:
        200:
          description: OK
  /some-items:
    delete:
      responses:
        200:
          description: OK
```

**Overlay:**

```yaml
overlay: 1.1.0
info:
  title: Use reusable actions with parameters and environment variables
  version: 1.0.0
x-components:
  actions:
    errorResponse:
      fields:
        target: "$.paths['%param.pathItem%'].%param.operation%.responses"
        update:
          404:
            description: Not Found
            content:
              application/json:
                schema:
                  type: object
                  properties:
                    '%param.propertyName%':
                      type: string
                    stageName:
                      type: string
                      const: '%env.stageName%'
        description: Adds an error response to the %param.pathItem% path item %param.operation% operation
      parameters:
        - name: pathItem
        - name: operation
          default: get
        - name: propertyName
          default: errorMessage
      environmentVariables:
        - name: stageName
          default: dev
actions:
  - x-$ref: '#/components/actions/errorResponse'
    x-parameterValues:
      pathItem: '/items'
  - x-$ref: '#/components/actions/errorResponse'
    x-parameterValues:
      pathItem: '/some-items'
      operation: delete
      propertyName: deleteErrorMessage
```

In this example:

- The `parameters` field defines values that can be interpolated using `%param.parameterName%` syntax in the reusable action's string fields (target, copy, description)
- The `environmentVariables` field defines references to process environment variables that can be interpolated using `%env.variableName%` syntax
- Default values are provided for both parameters and environment variables, used when not explicitly provided by the reference
- Each reference can supply different parameter values through the `parameterValues` object, allowing the same reusable action to target different paths and generate different content

## Release notes

The OpenAPI Overlay Libraries releases notes are available from the [CHANGELOG](CHANGELOG.md)

## Debugging

## Contributing

This project welcomes contributions and suggestions.  Make sure you open an issue before sending any pull request to avoid any misunderstanding.

## Trademarks
