FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["KYCService.Api/KYCService.Api.csproj", "KYCService.Api/"]
COPY ["KYCService.Application/KYCService.Application.csproj", "KYCService.Application/"]
COPY ["KYCService.Domain/KYCService.Domain.csproj", "KYCService.Domain/"]
COPY ["KYCService.Infrastructure/KYCService.Infrastructure.csproj", "KYCService.Infrastructure/"]
RUN dotnet restore "KYCService.Api/KYCService.Api.csproj"
COPY . .
WORKDIR "/src/KYCService.Api"
RUN dotnet build "KYCService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KYCService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KYCService.Api.dll"]
