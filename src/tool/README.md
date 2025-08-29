# CLIO - CLI Overlays

The BinkyLabs CLI overlays is a dotnet tool which can be used to apply OpenAPI overlays to OpenAPI descriptions.

## Installation

```shell
dotnet tool install -g BinkyLabs.OpenApi.Overlays.Tool --prerelease
```

## Usage

```shell
clio apply pathOrUrlToInputDescription --overlay pathOrUrlToOverlay -out pathForResultingDescription
```
