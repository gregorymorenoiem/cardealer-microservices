# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files y restore dependencies
COPY ["EventTrackingService.Api/EventTrackingService.Api.csproj", "EventTrackingService.Api/"]
COPY ["EventTrackingService.Application/EventTrackingService.Application.csproj", "EventTrackingService.Application/"]
COPY ["EventTrackingService.Domain/EventTrackingService.Domain.csproj", "EventTrackingService.Domain/"]
COPY ["EventTrackingService.Infrastructure/EventTrackingService.Infrastructure.csproj", "EventTrackingService.Infrastructure/"]

RUN dotnet restore "EventTrackingService.Api/EventTrackingService.Api.csproj"

# Copy source code
COPY . .

WORKDIR "/src/EventTrackingService.Api"
RUN dotnet build "EventTrackingService.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "EventTrackingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Expose port 8080 (Kubernetes standard)
EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "EventTrackingService.Api.dll"]
