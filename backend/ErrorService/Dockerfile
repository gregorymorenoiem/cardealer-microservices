# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["ErrorService/ErrorService.Api/ErrorService.Api.csproj", "ErrorService/ErrorService.Api/"]
COPY ["ErrorService/ErrorService.Application/ErrorService.Application.csproj", "ErrorService/ErrorService.Application/"]
COPY ["ErrorService/ErrorService.Domain/ErrorService.Domain.csproj", "ErrorService/ErrorService.Domain/"]
COPY ["ErrorService/ErrorService.Infrastructure/ErrorService.Infrastructure.csproj", "ErrorService/ErrorService.Infrastructure/"]

RUN dotnet restore "ErrorService/ErrorService.Api/ErrorService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ErrorService/ErrorService.Api"
RUN dotnet build "ErrorService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ErrorService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create non-root user (Alpine syntax)
RUN addgroup -g 1000 appuser && \
    adduser -D -u 1000 -G appuser appuser && \
    chown -R appuser:appuser /app

USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD dotnet /app/ErrorService.Api.dll --help > /dev/null 2>&1 || exit 1

ENTRYPOINT ["dotnet", "ErrorService.Api.dll"]
