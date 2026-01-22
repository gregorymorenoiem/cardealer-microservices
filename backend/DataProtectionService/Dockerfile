FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DataProtectionService.Api/DataProtectionService.Api.csproj", "DataProtectionService.Api/"]
COPY ["DataProtectionService.Application/DataProtectionService.Application.csproj", "DataProtectionService.Application/"]
COPY ["DataProtectionService.Domain/DataProtectionService.Domain.csproj", "DataProtectionService.Domain/"]
COPY ["DataProtectionService.Infrastructure/DataProtectionService.Infrastructure.csproj", "DataProtectionService.Infrastructure/"]
RUN dotnet restore "DataProtectionService.Api/DataProtectionService.Api.csproj"
COPY . .
WORKDIR "/src/DataProtectionService.Api"
RUN dotnet build "DataProtectionService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataProtectionService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DataProtectionService.Api.dll"]
