FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends wget && rm -rf /var/lib/apt/lists/*
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["ChatbotService/ChatbotService.Api/ChatbotService.Api.csproj", "ChatbotService.Api/"]
COPY ["ChatbotService/ChatbotService.Application/ChatbotService.Application.csproj", "ChatbotService.Application/"]
COPY ["ChatbotService/ChatbotService.Infrastructure/ChatbotService.Infrastructure.csproj", "ChatbotService.Infrastructure/"]
COPY ["ChatbotService/ChatbotService.Domain/ChatbotService.Domain.csproj", "ChatbotService.Domain/"]

# Restore dependencies
RUN dotnet restore "ChatbotService.Api/ChatbotService.Api.csproj"

# Copy everything else
COPY ChatbotService/ .

# Build
WORKDIR "/src/ChatbotService.Api"
RUN dotnet build "ChatbotService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatbotService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "ChatbotService.Api.dll"]
