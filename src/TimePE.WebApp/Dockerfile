# syntax=docker/dockerfile:1

# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and restore
COPY TimePE.sln ./
COPY src/TimePE.Core/TimePE.Core.csproj src/TimePE.Core/
COPY src/TimePE.WebApp/TimePE.WebApp.csproj src/TimePE.WebApp/
RUN dotnet restore TimePE.sln

# Copy source and publish
COPY src/ ./src/
RUN dotnet publish src/TimePE.WebApp/TimePE.WebApp.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "TimePE" timepe \
    && mkdir -p /app /data /logs \
    && chown -R timepe:timepe /app /data /logs

# Copy published app
COPY --from=build /app/publish .

# Environment variables
ENV ASPNETCORE_URLS=http://0.0.0.0:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=UTC

# Expose ports
EXPOSE 8080

# Run as non-root
USER timepe

# SQLite database path should be configurable via appsettings or env
# Default volume for persistent data
VOLUME ["/data", "/logs"]

# Entrypoint
ENTRYPOINT ["dotnet", "TimePE.WebApp.dll"]
