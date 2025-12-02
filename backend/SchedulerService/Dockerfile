# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["SchedulerService.Api/SchedulerService.Api.csproj", "SchedulerService.Api/"]
COPY ["SchedulerService.Infrastructure/SchedulerService.Infrastructure.csproj", "SchedulerService.Infrastructure/"]
COPY ["SchedulerService.Application/SchedulerService.Application.csproj", "SchedulerService.Application/"]
COPY ["SchedulerService.Domain/SchedulerService.Domain.csproj", "SchedulerService.Domain/"]

# Restore dependencies
RUN dotnet restore "SchedulerService.Api/SchedulerService.Api.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/SchedulerService.Api"
RUN dotnet build "SchedulerService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SchedulerService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create non-root user
RUN useradd -m -u 1000 schedulerservice && chown -R schedulerservice:schedulerservice /app
USER schedulerservice

COPY --from=publish --chown=schedulerservice:schedulerservice /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "SchedulerService.Api.dll"]
