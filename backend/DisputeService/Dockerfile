FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["DisputeService.Api/DisputeService.Api.csproj", "DisputeService.Api/"]
COPY ["DisputeService.Application/DisputeService.Application.csproj", "DisputeService.Application/"]
COPY ["DisputeService.Domain/DisputeService.Domain.csproj", "DisputeService.Domain/"]
COPY ["DisputeService.Infrastructure/DisputeService.Infrastructure.csproj", "DisputeService.Infrastructure/"]
RUN dotnet restore "DisputeService.Api/DisputeService.Api.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/DisputeService.Api"
RUN dotnet build "DisputeService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DisputeService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DisputeService.Api.dll"]
