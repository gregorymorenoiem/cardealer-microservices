# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["LoggingService/LoggingService.Api/LoggingService.Api.csproj", "LoggingService/LoggingService.Api/"]
COPY ["LoggingService/LoggingService.Application/LoggingService.Application.csproj", "LoggingService/LoggingService.Application/"]
COPY ["LoggingService/LoggingService.Domain/LoggingService.Domain.csproj", "LoggingService/LoggingService.Domain/"]
COPY ["LoggingService/LoggingService.Infrastructure/LoggingService.Infrastructure.csproj", "LoggingService/LoggingService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "LoggingService/LoggingService.Api/LoggingService.Api.csproj"

# Copy the rest of the code
COPY LoggingService/ LoggingService/

# Build
WORKDIR "/src/LoggingService/LoggingService.Api"
RUN dotnet build "LoggingService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "LoggingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LoggingService.Api.dll"]
