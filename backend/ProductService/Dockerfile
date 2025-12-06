# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["ProductService/ProductService.Api/ProductService.Api.csproj", "ProductService/ProductService.Api/"]
COPY ["ProductService/ProductService.Application/ProductService.Application.csproj", "ProductService/ProductService.Application/"]
COPY ["ProductService/ProductService.Domain/ProductService.Domain.csproj", "ProductService/ProductService.Domain/"]
COPY ["ProductService/ProductService.Infrastructure/ProductService.Infrastructure.csproj", "ProductService/ProductService.Infrastructure/"]
COPY ["ProductService/ProductService.Shared/ProductService.Shared.csproj", "ProductService/ProductService.Shared/"]

RUN dotnet restore "ProductService/ProductService.Api/ProductService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ProductService/ProductService.Api"
RUN dotnet build "ProductService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ProductService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

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
    CMD dotnet /app/ProductService.Api.dll --help > /dev/null 2>&1 || exit 1

ENTRYPOINT ["dotnet", "ProductService.Api.dll"]
