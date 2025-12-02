FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IdempotencyService.Api/IdempotencyService.Api.csproj", "IdempotencyService.Api/"]
COPY ["IdempotencyService.Core/IdempotencyService.Core.csproj", "IdempotencyService.Core/"]
RUN dotnet restore "IdempotencyService.Api/IdempotencyService.Api.csproj"
COPY . .
WORKDIR "/src/IdempotencyService.Api"
RUN dotnet build "IdempotencyService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IdempotencyService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "IdempotencyService.Api.dll"]
