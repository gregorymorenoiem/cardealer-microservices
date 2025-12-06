# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["InvoicingService.Api/InvoicingService.Api.csproj", "InvoicingService.Api/"]
COPY ["InvoicingService.Application/InvoicingService.Application.csproj", "InvoicingService.Application/"]
COPY ["InvoicingService.Domain/InvoicingService.Domain.csproj", "InvoicingService.Domain/"]
COPY ["InvoicingService.Infrastructure/InvoicingService.Infrastructure.csproj", "InvoicingService.Infrastructure/"]
COPY ["../_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "../_Shared/CarDealer.Shared/"]

RUN dotnet restore "InvoicingService.Api/InvoicingService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/InvoicingService.Api"
RUN dotnet build "InvoicingService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "InvoicingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InvoicingService.Api.dll"]
