FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5012
ENV ASPNETCORE_URLS=http://+:5012

# Create uploads directory
RUN mkdir -p /app/uploads && chmod 777 /app/uploads

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["FileStorageService.Api/FileStorageService.Api.csproj", "FileStorageService.Api/"]
COPY ["FileStorageService.Core/FileStorageService.Core.csproj", "FileStorageService.Core/"]

# Restore dependencies
RUN dotnet restore "FileStorageService.Api/FileStorageService.Api.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/FileStorageService.Api"
RUN dotnet build "FileStorageService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FileStorageService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "FileStorageService.Api.dll"]
