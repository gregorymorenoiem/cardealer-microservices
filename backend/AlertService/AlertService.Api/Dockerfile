FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backend/AlertService/AlertService.Api/AlertService.Api.csproj", "backend/AlertService/AlertService.Api/"]
COPY ["backend/AlertService/AlertService.Domain/AlertService.Domain.csproj", "backend/AlertService/AlertService.Domain/"]
COPY ["backend/AlertService/AlertService.Infrastructure/AlertService.Infrastructure.csproj", "backend/AlertService/AlertService.Infrastructure/"]
RUN dotnet restore "backend/AlertService/AlertService.Api/AlertService.Api.csproj"
COPY . .
WORKDIR "/src/backend/AlertService/AlertService.Api"
RUN dotnet build "AlertService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AlertService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AlertService.Api.dll"]
