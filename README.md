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

### Using Docker

You can run the CLI in a Docker container without installing .NET locally.

#### Using Pre-built Images

Docker images are automatically published to GitHub Container Registry:

```shell
# Pull and run the latest version
docker run --rm -v $(pwd)/output:/app/output \
  ghcr.io/binkylabs/openapi-overlays-dotnet:latest \
  apply /app/samples/description.yaml \
  --overlay /app/samples/overlay.yaml \
  -out /app/output/result.yaml
```

#### Building the Docker image

```shell
docker build -t clio:latest .
```

#### Quick Test with Built-in Samples

The Docker image includes sample files from `debug-samples/`. Run a quick test:

```shell
# Linux/Mac
./test-docker.sh

# Windows PowerShell
.\test-docker.ps1
```

Or manually:

```shell
# Create output directory
mkdir -p output

# Run with built-in samples
docker run --rm -v $(pwd)/output:/app/output clio:latest \
  apply /app/samples/description.yaml \
  --overlay /app/samples/overlay.yaml \
  -out /app/output/result.yaml

# Check the result
cat output/result.yaml
```

Or use docker-compose:

```shell
# Uses built-in samples by default
docker-compose run --rm clio

# Check the result
cat output/result.yaml
```

#### Running with Docker

```shell
# Apply an overlay using mounted volumes
docker run --rm \
  -v $(pwd)/input:/app/input:ro \
  -v $(pwd)/output:/app/output \
  clio:latest apply /app/input/openapi.yaml \
    --overlay /app/input/overlay.yaml \
    -out /app/output/result.yaml
```

#### Using docker-compose

You can also use the included `docker-compose.yml` file:

```shell
# Run with built-in samples (default)
docker-compose run --rm clio

# Or run with custom files using clio-custom service
docker-compose run --rm clio-custom

# Or override the command
docker-compose run --rm clio apply /app/samples/description.yaml \
  --overlay /app/samples/overlay.yaml \
  -out /app/output/custom-result.yaml
```

> For detailed Docker instructions, see [DOCKER.md](DOCKER.md) or [DOCKER-QUICKSTART.md](DOCKER-QUICKSTART.md)

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

## Release notes

The OpenAPI Overlay Libraries releases notes are available from the [CHANGELOG](CHANGELOG.md)

## Debugging

## Contributing

This project welcomes contributions and suggestions.  Make sure you open an issue before sending any pull request to avoid any misunderstanding.

## Trademarks

