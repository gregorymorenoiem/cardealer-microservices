FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["ChatbotService/ChatbotService.Api/ChatbotService.Api.csproj", "ChatbotService/ChatbotService.Api/"]
COPY ["ChatbotService/ChatbotService.Application/ChatbotService.Application.csproj", "ChatbotService/ChatbotService.Application/"]
COPY ["ChatbotService/ChatbotService.Domain/ChatbotService.Domain.csproj", "ChatbotService/ChatbotService.Domain/"]
COPY ["ChatbotService/ChatbotService.Infrastructure/ChatbotService.Infrastructure.csproj", "ChatbotService/ChatbotService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ChatbotService/ChatbotService.Api/ChatbotService.Api.csproj"

# Copy all files
COPY . .

# Build
WORKDIR "/src/ChatbotService/ChatbotService.Api"
RUN dotnet build "ChatbotService.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ChatbotService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatbotService.Api.dll"]
