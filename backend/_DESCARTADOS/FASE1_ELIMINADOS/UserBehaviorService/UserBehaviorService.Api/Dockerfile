FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["UserBehaviorService.Api/UserBehaviorService.Api.csproj", "UserBehaviorService.Api/"]
COPY ["UserBehaviorService.Application/UserBehaviorService.Application.csproj", "UserBehaviorService.Application/"]
COPY ["UserBehaviorService.Domain/UserBehaviorService.Domain.csproj", "UserBehaviorService.Domain/"]
COPY ["UserBehaviorService.Infrastructure/UserBehaviorService.Infrastructure.csproj", "UserBehaviorService.Infrastructure/"]
RUN dotnet restore "UserBehaviorService.Api/UserBehaviorService.Api.csproj"
COPY . .
WORKDIR "/src/UserBehaviorService.Api"
RUN dotnet build "UserBehaviorService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserBehaviorService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserBehaviorService.Api.dll"]
