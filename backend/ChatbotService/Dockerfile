FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["ChatbotService.Api/ChatbotService.Api.csproj", "ChatbotService.Api/"]
COPY ["ChatbotService.Application/ChatbotService.Application.csproj", "ChatbotService.Application/"]
COPY ["ChatbotService.Domain/ChatbotService.Domain.csproj", "ChatbotService.Domain/"]
COPY ["ChatbotService.Infrastructure/ChatbotService.Infrastructure.csproj", "ChatbotService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ChatbotService.Api/ChatbotService.Api.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/ChatbotService.Api"
RUN dotnet build "ChatbotService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatbotService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs && chmod 755 /app/logs

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ChatbotService.Api.dll"]
