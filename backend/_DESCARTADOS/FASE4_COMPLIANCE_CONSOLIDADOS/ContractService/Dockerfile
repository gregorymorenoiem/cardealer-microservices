# ContractService - Ley 126-02 Electronic Contracts
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["ContractService.Api/ContractService.Api.csproj", "ContractService.Api/"]
COPY ["ContractService.Application/ContractService.Application.csproj", "ContractService.Application/"]
COPY ["ContractService.Domain/ContractService.Domain.csproj", "ContractService.Domain/"]
COPY ["ContractService.Infrastructure/ContractService.Infrastructure.csproj", "ContractService.Infrastructure/"]

# Restore
RUN dotnet restore "ContractService.Api/ContractService.Api.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/ContractService.Api"
RUN dotnet build "ContractService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ContractService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ContractService.Api.dll"]
