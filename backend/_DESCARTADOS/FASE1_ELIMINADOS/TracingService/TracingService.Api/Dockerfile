FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["TracingService.Api/TracingService.Api.csproj", "TracingService.Api/"]
COPY ["TracingService.Application/TracingService.Application.csproj", "TracingService.Application/"]
COPY ["TracingService.Domain/TracingService.Domain.csproj", "TracingService.Domain/"]
COPY ["TracingService.Infrastructure/TracingService.Infrastructure.csproj", "TracingService.Infrastructure/"]

RUN dotnet restore "TracingService.Api/TracingService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/TracingService.Api"
RUN dotnet build "TracingService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TracingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "TracingService.Api.dll"]
