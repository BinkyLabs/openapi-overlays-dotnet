# CLIO - CLI Overlays

The BinkyLabs CLI overlays is a dotnet tool which can be used to apply OpenAPI overlays to OpenAPI descriptions.

## Installation

```shell
dotnet tool install -g BinkyLabs.OpenApi.Overlays.Tool
```

## Usage

### Apply an overlay to an OpenAPI description

The apply command applies the overlay actions to an OpenAPI description and preserves the source ordering of fields.

```shell
clio apply pathOrUrlToInputDescription --overlay pathOrUrlToOverlay -out pathForResultingDescription
```

> Note: the overlay argument can be specified multiple times, the order matters.

### Apply and normalize

The apply command applies the overlay actions to an OpenAPI description and normalizes the description based on OpenAPI.net rules and fields ordering.

```shell
clio apply-and-normalize pathOrUrlToInputDescription --overlay pathOrUrlToOverlay -out pathForResultingDescription
```

> Note: the overlay argument can be specified multiple times, the order matters.
