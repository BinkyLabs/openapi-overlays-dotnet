# Docker Support for OpenAPI Overlays CLI

This document provides detailed instructions for using the OpenAPI Overlays CLI with Docker.

## Prerequisites

- Docker installed on your system
- (Optional) Docker Compose for simplified orchestration

## Using Pre-built Images

Docker images are automatically built and published to GitHub Container Registry on every push to main and on tagged releases.

### Pull the Latest Image

```bash
# Pull the latest version from main branch
docker pull ghcr.io/binkylabs/openapi-overlays-dotnet:latest

# Pull a specific version (replace with actual version)
docker pull ghcr.io/binkylabs/openapi-overlays-dotnet:1.0.0
```

### Using the Pre-built Image

```bash
# Run with the pre-built image
docker run --rm -v $(pwd)/output:/app/output \
  ghcr.io/binkylabs/openapi-overlays-dotnet:latest \
  apply /app/samples/description.yaml \
  --overlay /app/samples/overlay.yaml \
  -out /app/output/result.yaml
```

## Quick Start with Built-in Samples

The Docker image includes sample files from the `debug-samples/` directory for quick testing.

### Using Docker

```bash
# Build the image locally
docker build -t clio:latest .

# Create output directory
mkdir -p output

# Run with built-in samples
docker run --rm -v $(pwd)/output:/app/output clio:latest \
  apply /app/samples/description.yaml \
  --overlay /app/samples/overlay.yaml \
  -out /app/output/result.yaml

# View the result
cat output/result.yaml
```

### Using Docker Compose

```bash
# Run with built-in samples (default command)
docker-compose run --rm clio

# View the result
cat output/result.yaml
```

The built-in samples demonstrate:
- **description.yaml**: A simple OpenAPI 3.1.0 test API
- **overlay.yaml**: An overlay that removes the description field from the API info

## Building the Image

### Standard Build

Build the Docker image using the default settings:

```bash
docker build -t clio:latest .
```

### Build with Version Suffix

Build with a custom version suffix (e.g., for pre-release versions):

```bash
docker build --build-arg version_suffix=preview.1 -t clio:preview .
```

### Multi-platform Build

Build for multiple platforms (requires buildx):

```bash
docker buildx build --platform linux/amd64,linux/arm64 -t clio:latest .
```

## Running the Container

### Using Built-in Samples

The Docker image includes sample files at `/app/samples/`:
- `/app/samples/description.yaml` - Sample OpenAPI description
- `/app/samples/overlay.yaml` - Sample overlay

```bash
# Basic usage with samples
docker run --rm -v $(pwd)/output:/app/output clio:latest \
  apply /app/samples/description.yaml \
  --overlay /app/samples/overlay.yaml \
  -out /app/output/result.yaml

# Apply and normalize with samples
docker run --rm -v $(pwd)/output:/app/output clio:latest \
  apply-and-normalize /app/samples/description.yaml \
  --overlay /app/samples/overlay.yaml \
  -out /app/output/normalized.yaml
```

### Basic Usage

Apply an overlay to an OpenAPI document:

```bash
docker run --rm \
  -v $(pwd)/examples:/app/examples:ro \
  -v $(pwd)/output:/app/output \
  clio:latest apply /app/examples/openapi.yaml \
    --overlay /app/examples/overlay.yaml \
    -out /app/output/result.yaml
```

### Apply Multiple Overlays

```bash
docker run --rm \
  -v $(pwd)/examples:/app/examples:ro \
  -v $(pwd)/output:/app/output \
  clio:latest apply /app/examples/openapi.yaml \
    --overlay /app/examples/overlay1.yaml \
    --overlay /app/examples/overlay2.yaml \
    -out /app/output/result.yaml
```

### Apply and Normalize

```bash
docker run --rm \
  -v $(pwd)/examples:/app/examples:ro \
  -v $(pwd)/output:/app/output \
  clio:latest apply-and-normalize /app/examples/openapi.yaml \
    --overlay /app/examples/overlay.yaml \
    -out /app/output/result.yaml
```

### Working with URLs

The CLI can also fetch documents from URLs:

```bash
docker run --rm \
  -v $(pwd)/output:/app/output \
  clio:latest apply https://example.com/openapi.yaml \
    --overlay https://example.com/overlay.yaml \
    -out /app/output/result.yaml
```

## Using Docker Compose

### Basic Setup

1. Edit the `docker-compose.yml` file to configure your volumes:

```yaml
version: '3.8'

services:
  clio:
    build:
      context: .
      dockerfile: Dockerfile
    volumes:
      - ./examples/openapi.yaml:/app/openapi.yaml:ro
      - ./examples/overlay.yaml:/app/overlay.yaml:ro
      - ./output:/app/output
    command: apply /app/openapi.yaml --overlay /app/overlay.yaml -out /app/output/result.yaml
```

2. Run with docker-compose:

```bash
docker-compose run --rm clio
```

### Custom Commands

Override the default command:

```bash
docker-compose run --rm clio apply-and-normalize /app/openapi.yaml \
  --overlay /app/overlay.yaml \
  -out /app/output/normalized.yaml
```

## Volume Mounts

The Dockerfile defines the following volumes:

- `/app/output` - For output files
- `/app/openapi.yaml` - For the OpenAPI description (can be overridden)
- `/app/overlay.yaml` - For the overlay file (can be overridden)

You can mount your local directories to these paths or use custom paths in your commands.

## Environment Variables

The container sets the following environment variables:

- `CLIO_CONTAINER=true` - Indicates the CLI is running in a container
- `DOTNET_TieredPGO=1` - Enables tiered compilation profile-guided optimization
- `DOTNET_TC_QuickJitForLoops=1` - Enables quick JIT for loops

## Image Details

- Base SDK Image: `mcr.microsoft.com/dotnet/sdk:10.0`
- Runtime Image: `mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled-extra`
- Target Framework: .NET 10.0
- Platform: Multi-platform support (linux/amd64, linux/arm64)

## Troubleshooting

### Permission Issues

If you encounter permission issues with output files, ensure the output directory has appropriate permissions:

```bash
mkdir -p output
chmod 777 output  # Or use appropriate permissions for your environment
```

### Volume Mount Issues on Windows

On Windows, use PowerShell and adjust the volume paths:

```powershell
docker run --rm `
  -v ${PWD}/examples:/app/examples:ro `
  -v ${PWD}/output:/app/output `
  clio:latest apply /app/examples/openapi.yaml `
    --overlay /app/examples/overlay.yaml `
    -out /app/output/result.yaml
```

## Publishing the Image

To publish the image to a container registry:

```bash
# Tag the image
docker tag clio:latest yourusername/clio:latest
docker tag clio:latest yourusername/clio:1.0.0

# Push to Docker Hub
docker push yourusername/clio:latest
docker push yourusername/clio:1.0.0

# Or push to GitHub Container Registry
docker tag clio:latest ghcr.io/binkylabs/clio:latest
docker push ghcr.io/binkylabs/clio:latest
```

## CI/CD Integration

Docker images are automatically built and published as part of the CI/CD pipeline.

### Automated Builds

The Docker image is built on:
- Every push to the `main` branch (tagged as `latest`)
- Every pull request (for testing, not published)
- Every tagged release (e.g., `v1.0.0`, tagged as `1.0.0`, `1.0`, `1`)

### GitHub Container Registry

Images are published to: `ghcr.io/binkylabs/openapi-overlays-dotnet`

Available tags:
- `latest` - Latest build from main branch
- `main` - Latest build from main branch
- `v1.0.0` - Specific version tag
- `1.0.0` - Semantic version
- `1.0` - Major.minor version
- `1` - Major version

### Using in Your Workflow

```yaml
name: Apply OpenAPI Overlays

on: [push]

jobs:
  apply-overlays:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Apply overlay
        run: |
          docker run --rm \
            -v ${{ github.workspace }}/api:/app/api:ro \
            -v ${{ github.workspace }}/output:/app/output \
            ghcr.io/binkylabs/openapi-overlays-dotnet:latest \
            apply /app/api/openapi.yaml \
            --overlay /app/api/overlay.yaml \
            -out /app/output/result.yaml
      
      - name: Upload result
        uses: actions/upload-artifact@v4
        with:
          name: openapi-result
          path: output/result.yaml
```

### Build Provenance

All published images include build provenance attestations for supply chain security. You can verify the attestation using:

```bash
docker buildx imagetools inspect ghcr.io/binkylabs/openapi-overlays-dotnet:latest --format "{{ json .Provenance }}"
```

## Best Practices

1. **Use Read-Only Mounts**: Mount input files as read-only (`:ro`) to prevent accidental modifications
2. **Version Your Images**: Tag images with specific versions for reproducibility
3. **Use Multi-stage Builds**: The Dockerfile uses multi-stage builds to minimize image size
4. **Clean Up**: Use `--rm` flag to automatically remove containers after execution
5. **Security**: Run containers with appropriate user permissions (consider adding `--user` flag)

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [OpenAPI Overlays Specification](https://spec.openapis.org/overlay/latest.html)
- [Project Repository](https://github.com/BinkyLabs/openapi-overlays-dotnet)
