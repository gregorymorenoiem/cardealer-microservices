FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backend/ComparisonService/ComparisonService.Api/ComparisonService.Api.csproj", "backend/ComparisonService/ComparisonService.Api/"]
COPY ["backend/ComparisonService/ComparisonService.Domain/ComparisonService.Domain.csproj", "backend/ComparisonService/ComparisonService.Domain/"]
COPY ["backend/ComparisonService/ComparisonService.Infrastructure/ComparisonService.Infrastructure.csproj", "backend/ComparisonService/ComparisonService.Infrastructure/"]
RUN dotnet restore "backend/ComparisonService/ComparisonService.Api/ComparisonService.Api.csproj"
COPY . .
WORKDIR "/src/backend/ComparisonService/ComparisonService.Api"
RUN dotnet build "ComparisonService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ComparisonService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ComparisonService.Api.dll"]
