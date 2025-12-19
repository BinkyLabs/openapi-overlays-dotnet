# Quick Docker Test with Debug Samples

This guide shows you how to quickly test the Docker setup using the included debug samples.

## The Sample Files

The image includes two test files:

### description.yaml
A minimal OpenAPI 3.1.0 API with:
- Title: "Test API"
- Version: "1.0.0"
- Description: "Test Description"
- A single GET endpoint at `/test`

### overlay.yaml
A simple overlay that:
- Removes the `description` field from `$.info.description`

## Quick Test Commands

### Method 1: Docker Run (Simplest)

```bash
# Build the image
docker build -t clio:latest .

# Create output directory
mkdir -p output

# Run the test
docker run --rm -v $(pwd)/output:/app/output clio:latest \
  apply /app/samples/description.yaml \
  --overlay /app/samples/overlay.yaml \
  -out /app/output/result.yaml

# View the result
cat output/result.yaml
```

### Method 2: Docker Compose (Easiest)

```bash
# Run with default configuration (uses built-in samples)
docker-compose run --rm clio

# View the result
cat output/result.yaml
```

### Method 3: Interactive Shell

```bash
# Start a shell in the container
docker run --rm -it -v $(pwd)/output:/app/output \
  --entrypoint /bin/sh clio:latest

# Inside the container, run commands
dotnet BinkyLabs.OpenApi.Overlays.Cli.dll apply \
  /app/samples/description.yaml \
  --overlay /app/samples/overlay.yaml \
  -out /app/output/result.yaml

# Exit the container
exit
```

## Expected Result

After running the overlay, the `result.yaml` should look like:

```yaml
openapi: 3.1.0
info:
  title: Test API
  version: 1.0.0
  # Note: description field is removed
paths:
  /test:
    get:
      summary: Test endpoint
      responses:
        '200':
          description: OK
```

The `description: Test Description` line should be removed from the `info` section.

## Testing Different Commands

### Apply and Normalize

```bash
docker run --rm -v $(pwd)/output:/app/output clio:latest \
  apply-and-normalize /app/samples/description.yaml \
  --overlay /app/samples/overlay.yaml \
  -out /app/output/normalized.yaml
```

### Multiple Overlays (Using Samples Twice)

```bash
docker run --rm -v $(pwd)/output:/app/output clio:latest \
  apply /app/samples/description.yaml \
  --overlay /app/samples/overlay.yaml \
  --overlay /app/samples/overlay.yaml \
  -out /app/output/result.yaml
```

## Troubleshooting

### Output directory doesn't exist
```bash
mkdir -p output
```

### Permission issues on Linux/Mac
```bash
chmod 777 output
```

### Windows PowerShell
```powershell
# Build
docker build -t clio:latest .

# Run
docker run --rm -v ${PWD}/output:/app/output clio:latest `
  apply /app/samples/description.yaml `
  --overlay /app/samples/overlay.yaml `
  -out /app/output/result.yaml

# View result
Get-Content output/result.yaml
```

## Next Steps

Once the test works:
1. Try with your own OpenAPI files by mounting them as volumes
2. See [DOCKER.md](DOCKER.md) for advanced usage
3. Read [README.md](README.md) for CLI documentation
