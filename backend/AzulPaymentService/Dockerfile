# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects
COPY _Shared/CarDealer.Shared/CarDealer.Shared.csproj _Shared/CarDealer.Shared/
COPY _Shared/CarDealer.Contracts/CarDealer.Contracts.csproj _Shared/CarDealer.Contracts/
# Copy service projects
COPY AzulPaymentService/AzulPaymentService.Domain/AzulPaymentService.Domain.csproj AzulPaymentService/AzulPaymentService.Domain/
COPY AzulPaymentService/AzulPaymentService.Application/AzulPaymentService.Application.csproj AzulPaymentService/AzulPaymentService.Application/
COPY AzulPaymentService/AzulPaymentService.Infrastructure/AzulPaymentService.Infrastructure.csproj AzulPaymentService/AzulPaymentService.Infrastructure/
COPY AzulPaymentService/AzulPaymentService.Api/AzulPaymentService.Api.csproj AzulPaymentService/AzulPaymentService.Api/

# Restore dependencies
WORKDIR /src/AzulPaymentService/AzulPaymentService.Api
RUN dotnet restore

# Copy all source code
WORKDIR /src
COPY _Shared/CarDealer.Shared/ _Shared/CarDealer.Shared/
COPY _Shared/CarDealer.Contracts/ _Shared/CarDealer.Contracts/
COPY AzulPaymentService/ AzulPaymentService/

# Build the application
WORKDIR /src/AzulPaymentService/AzulPaymentService.Api
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install wget for health checks and create secrets directory
RUN apt-get update && apt-get install -y --no-install-recommends wget \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /run/secrets

# Create non-root user for security
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Docker

# Change ownership and switch to non-root user
RUN chown -R appuser:appgroup /app /run/secrets
USER appuser

# Expose port
EXPOSE 80

# Health check using wget
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "AzulPaymentService.Api.dll"]
