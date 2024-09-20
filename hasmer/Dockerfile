FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY hasmer.sln hasmer.sln
COPY hasmer-lsp/hasmer-lsp.csproj hasmer-lsp/hasmer-lsp.csproj
COPY hasmer-cli/hasmer-cli.csproj hasmer-cli/hasmer-cli.csproj
COPY libhasmer/libhasmer.csproj libhasmer/libhasmer.csproj
RUN dotnet restore "hasmer.sln"

COPY . .
RUN dotnet publish -o /app


FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /assets
COPY --from=build /app /app
ENTRYPOINT ["/app/hasmer"]
