# GitHub Action - OpenAPI Overlays

This GitHub Action allows you to apply OpenAPI overlays to OpenAPI documents in your CI/CD workflows using the BinkyLabs OpenAPI Overlays CLI.

## Quick Start

```yaml
- uses: BinkyLabs/openapi-overlays-dotnet@v2
  with:
    input: 'openapi.yaml'
    overlays: 'overlay.yaml'
    output: 'openapi-modified.yaml'
```

## Features

- Apply one or multiple overlays to OpenAPI documents
- Support for both YAML and JSON formats
- Two application modes: `apply` (preserves field ordering) and `apply-and-normalize` (normalizes with OpenAPI.net rules)
- Runs in Docker container - no .NET runtime installation required

## Usage

### Basic Example

```yaml
- name: Apply OpenAPI Overlays
  uses: BinkyLabs/openapi-overlays-dotnet@v2
  with:
    input: 'openapi.yaml'
    overlays: 'overlay.yaml'
    output: 'openapi-modified.yaml'
```

### Multiple Overlays

You can apply multiple overlays in sequence. The order matters - overlays are applied in the order specified:

```yaml
- name: Apply Multiple Overlays
  uses: BinkyLabs/openapi-overlays-dotnet@v2
  with:
    input: 'openapi.yaml'
    overlays: |
      overlays/base.yaml
      overlays/security.yaml
      overlays/examples.yaml
    output: 'openapi-complete.yaml'
```

### Using Apply and Normalize

The `apply-and-normalize` command applies overlays and then normalizes the OpenAPI document according to OpenAPI.net rules:

```yaml
- name: Apply and Normalize
  uses: BinkyLabs/openapi-overlays-dotnet@v2
  with:
    input: 'openapi.yaml'
    overlays: 'overlay.yaml'
    output: 'openapi-normalized.yaml'
    command: 'apply-and-normalize'
```

### Using Strict Mode

Enable strict mode to treat targets that match zero nodes as errors instead of warnings. This is useful for catching configuration issues in CI/CD:

```yaml
- name: Apply with Strict Mode
  uses: BinkyLabs/openapi-overlays-dotnet@v2
  with:
    input: 'openapi.yaml'
    overlays: 'overlay.yaml'
    output: 'openapi-modified.yaml'
    strict: 'true'
```

## Inputs

| Input | Description | Required | Default |
|-------|-------------|----------|---------|
| `input` | Path to the input OpenAPI document (YAML or JSON) | Yes | - |
| `overlays` | Paths to overlay file(s), separated by newlines or spaces. Multiple overlays will be applied in order. | Yes | - |
| `output` | Path for the output file | Yes | - |
| `command` | Command to run: `apply` (preserves field ordering) or `apply-and-normalize` (normalizes with OpenAPI.net rules) | No | `apply` |
| `force` | Overwrite output file without confirmation | No | `true` |
| `strict` | Treat targets that match zero nodes as errors instead of warnings | No | `false` |

## Complete Workflow Example

```yaml
name: Apply OpenAPI Overlays

on:
  push:
    branches: [main]
    paths:
      - 'specs/**'
      - 'overlays/**'
  pull_request:
    paths:
      - 'specs/**'
      - 'overlays/**'

jobs:
  generate-api-spec:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout repository
        uses: actions/checkout@v6

      - name: Apply OpenAPI Overlays
        uses: BinkyLabs/openapi-overlays-dotnet@v2
        with:
          input: 'specs/openapi.yaml'
          overlays: |
            overlays/production.yaml
            overlays/security.yaml
          output: 'dist/openapi-prod.yaml'

      - name: Upload Generated Spec
        uses: actions/upload-artifact@v6
        with:
          name: openapi-spec
          path: dist/openapi-prod.yaml

      - name: Commit Changes
        if: github.ref == 'refs/heads/main'
        run: |
          git config user.name "GitHub Actions"
          git config user.email "actions@github.com"
          git add dist/openapi-prod.yaml
          git diff --quiet && git diff --staged --quiet || git commit -m "chore: update generated OpenAPI spec"
          git push
```

## Advanced Usage

### Environment-Specific Overlays

```yaml
jobs:
  generate-staging-spec:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v6
      - uses: BinkyLabs/openapi-overlays-dotnet@v2
        with:
          input: 'openapi.yaml'
          overlays: |
            overlays/base.yaml
            overlays/staging.yaml
          output: 'openapi-staging.yaml'

  generate-production-spec:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v6
      - uses: BinkyLabs/openapi-overlays-dotnet@v2
        with:
          input: 'openapi.yaml'
          overlays: |
            overlays/base.yaml
            overlays/production.yaml
          output: 'openapi-production.yaml'
```

### Matrix Strategy for Multiple Environments

```yaml
jobs:
  generate-specs:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        environment: [dev, staging, production]
    steps:
      - uses: actions/checkout@v6
      - uses: BinkyLabs/openapi-overlays-dotnet@v2
        with:
          input: 'openapi.yaml'
          overlays: |
            overlays/base.yaml
            overlays/${{ matrix.environment }}.yaml
          output: 'dist/openapi-${{ matrix.environment }}.yaml'
      - uses: actions/upload-artifact@v6
        with:
          name: openapi-${{ matrix.environment }}
          path: dist/openapi-${{ matrix.environment }}.yaml
```

## Troubleshooting

### File Not Found Errors

Make sure all paths are relative to your repository root. The action mounts your workspace at `/workspace` inside the container.

### Permission Issues

If you encounter permission issues when writing output files, ensure the output directory exists and is writable. The action will create the output directory if it doesn't exist.

### Docker Pull Issues

The action uses `ghcr.io/binkylabs/openapi-overlays-dotnet:latest`. If you encounter pull issues, verify that:
- Your runner has internet access
- The GitHub Container Registry is accessible
- The image exists and is public

## Notes

- The action uses the Docker image `ghcr.io/binkylabs/openapi-overlays-dotnet:latest` which is automatically pulled if not present
- All file paths are relative to your repository root (`${{ github.workspace }}`)
- The action mounts your workspace as `/workspace` inside the container
- Output files are created in your workspace and can be committed or uploaded as artifacts

## Related Resources

- [OpenAPI Overlay Specification](https://spec.openapis.org/overlay/latest.html)
- [Project Repository](https://github.com/BinkyLabs/openapi-overlays-dotnet)
- [Docker Image](https://ghcr.io/binkylabs/openapi-overlays-dotnet)
- [NuGet Package](https://www.nuget.org/packages/BinkyLabs.OpenApi.Overlays)

## License

This action uses the BinkyLabs OpenAPI Overlays tool.
See the [project repository](https://github.com/BinkyLabs/openapi-overlays-dotnet) for license information.
