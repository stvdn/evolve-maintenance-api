# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files
COPY MSApiMantenimientosEvolutivos.sln ./
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/WebApi/WebApi.csproj src/WebApi/

# Restore dependencies
RUN dotnet restore src/WebApi/WebApi.csproj

# Copy source code
COPY src/ src/

# Build and publish
RUN dotnet publish src/WebApi/WebApi.csproj -c Release -o /out --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /out .

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "WebApi.dll"]
