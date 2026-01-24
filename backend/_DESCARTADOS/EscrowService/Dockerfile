FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["EscrowService.Api/EscrowService.Api.csproj", "EscrowService.Api/"]
COPY ["EscrowService.Application/EscrowService.Application.csproj", "EscrowService.Application/"]
COPY ["EscrowService.Domain/EscrowService.Domain.csproj", "EscrowService.Domain/"]
COPY ["EscrowService.Infrastructure/EscrowService.Infrastructure.csproj", "EscrowService.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "EscrowService.Api/EscrowService.Api.csproj"

# Copiar código fuente
COPY . .

# Build
WORKDIR "/src/EscrowService.Api"
RUN dotnet build "EscrowService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EscrowService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EscrowService.Api.dll"]
