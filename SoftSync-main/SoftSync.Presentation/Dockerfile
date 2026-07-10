# syntax=docker/dockerfile:1
# Multi-stage build for the SoftSync Blazor Server app.
# Build context is the REPOSITORY ROOT (so all four projects are visible).
#   docker build -f SoftSync.Presentation/Dockerfile -t softsync .

# ---- Build stage: .NET SDK + Node (for the Vite/Tailwind front-end build) ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Node 20 is needed so the csproj NpmInstall/ViteBuild targets can emit wwwroot/dist.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates gnupg \
    && curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y --no-install-recommends nodejs \
    && rm -rf /var/lib/apt/lists/*

# Restore first (better layer caching): copy only project files.
COPY SoftSync.sln ./
COPY SoftSync.Common/SoftSync.Common.csproj SoftSync.Common/
COPY SoftSync.DAL/SoftSync.DAL.csproj SoftSync.DAL/
COPY SoftSync.BLL/SoftSync.BLL.csproj SoftSync.BLL/
COPY SoftSync.Presentation/SoftSync.Presentation.csproj SoftSync.Presentation/
RUN dotnet restore SoftSync.Presentation/SoftSync.Presentation.csproj

# Copy the rest of the source and publish.
COPY . .
RUN dotnet publish SoftSync.Presentation/SoftSync.Presentation.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage: ASP.NET runtime only ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

# Render (and most PaaS) inject the port to listen on via $PORT. Bind to it,
# falling back to 8080 for local `docker run`.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["/bin/sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet SoftSync.Presentation.dll"]
