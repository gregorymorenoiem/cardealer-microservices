# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["ContactService/ContactService.Api/ContactService.Api.csproj", "ContactService/ContactService.Api/"]
COPY ["ContactService/ContactService.Application/ContactService.Application.csproj", "ContactService/ContactService.Application/"]
COPY ["ContactService/ContactService.Domain/ContactService.Domain.csproj", "ContactService/ContactService.Domain/"]
COPY ["ContactService/ContactService.Infrastructure/ContactService.Infrastructure.csproj", "ContactService/ContactService.Infrastructure/"]

RUN dotnet restore "ContactService/ContactService.Api/ContactService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ContactService/ContactService.Api"
RUN dotnet build "ContactService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ContactService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "ContactService.Api.dll"]
