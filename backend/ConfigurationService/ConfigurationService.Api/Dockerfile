FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ConfigurationService/ConfigurationService.Api/ConfigurationService.Api.csproj", "ConfigurationService/ConfigurationService.Api/"]
COPY ["ConfigurationService/ConfigurationService.Application/ConfigurationService.Application.csproj", "ConfigurationService/ConfigurationService.Application/"]
COPY ["ConfigurationService/ConfigurationService.Domain/ConfigurationService.Domain.csproj", "ConfigurationService/ConfigurationService.Domain/"]
COPY ["ConfigurationService/ConfigurationService.Infrastructure/ConfigurationService.Infrastructure.csproj", "ConfigurationService/ConfigurationService.Infrastructure/"]
RUN dotnet restore "ConfigurationService/ConfigurationService.Api/ConfigurationService.Api.csproj"
COPY . .
WORKDIR "/src/ConfigurationService/ConfigurationService.Api"
RUN dotnet build "ConfigurationService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConfigurationService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConfigurationService.Api.dll"]
