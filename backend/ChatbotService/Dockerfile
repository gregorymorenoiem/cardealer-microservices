FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["ChatbotService.Api/ChatbotService.Api.csproj", "ChatbotService.Api/"]
COPY ["ChatbotService.Application/ChatbotService.Application.csproj", "ChatbotService.Application/"]
COPY ["ChatbotService.Infrastructure/ChatbotService.Infrastructure.csproj", "ChatbotService.Infrastructure/"]
COPY ["ChatbotService.Domain/ChatbotService.Domain.csproj", "ChatbotService.Domain/"]

# Restore dependencies
RUN dotnet restore "ChatbotService.Api/ChatbotService.Api.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/ChatbotService.Api"
RUN dotnet build "ChatbotService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatbotService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ChatbotService.Api.dll"]
