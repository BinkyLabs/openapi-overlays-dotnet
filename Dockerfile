FROM --platform=${BUILDPLATFORM} mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
ARG version_suffix
WORKDIR /app

COPY ./src ./clio/src
COPY ./tests ./clio/tests
COPY ./keyfile.snk ./clio/keyfile.snk
COPY ./README.md ./clio/README.md
COPY ./BinkyLabs.OpenAPI.Overlays.slnx ./clio/BinkyLabs.OpenAPI.Overlays.slnx
WORKDIR /app/clio
RUN if [ -z "$version_suffix" ]; then \
    dotnet publish ./src/tool/BinkyLabs.OpenApi.Overlays.Cli.csproj -c Release -p:TreatWarningsAsErrors=false -f net10.0; \
    else \
    dotnet publish ./src/tool/BinkyLabs.OpenApi.Overlays.Cli.csproj -c Release -p:TreatWarningsAsErrors=false -f net10.0 --version-suffix "$version_suffix"; \
    fi

# Don't use the chiseled image without extras 
# (see https://github.com/microsoft/kiota/issues/4600)
FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled-extra AS runtime
WORKDIR /app

COPY --from=build-env /app/clio/src/tool/bin/Release/net10.0/publish ./

# Copy sample files for testing
COPY ./debug-samples /app/samples

VOLUME /app/output
VOLUME /app/openapi.yaml
VOLUME /app/overlay.yaml
ENV CLIO_CONTAINER=true DOTNET_TieredPGO=1 DOTNET_TC_QuickJitForLoops=1
ENTRYPOINT ["dotnet", "BinkyLabs.OpenApi.Overlays.Cli.dll"]
LABEL description="# Welcome to OpenAPI Overlays CLI \
    To start applying overlays to OpenAPI documents checkout [the documentation](https://github.com/BinkyLabs/openapi-overlays-dotnet)  \
    [Source dockerfile](https://github.com/BinkyLabs/openapi-overlays-dotnet/blob/main/Dockerfile)"
