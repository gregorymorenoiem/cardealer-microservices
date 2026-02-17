FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore
COPY ["backend/StaffService/StaffService.Api/StaffService.Api.csproj", "backend/StaffService/StaffService.Api/"]
COPY ["backend/StaffService/StaffService.Application/StaffService.Application.csproj", "backend/StaffService/StaffService.Application/"]
COPY ["backend/StaffService/StaffService.Domain/StaffService.Domain.csproj", "backend/StaffService/StaffService.Domain/"]
COPY ["backend/StaffService/StaffService.Infrastructure/StaffService.Infrastructure.csproj", "backend/StaffService/StaffService.Infrastructure/"]
COPY ["backend/_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "backend/_Shared/CarDealer.Contracts/"]
COPY ["backend/_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "backend/_Shared/CarDealer.Shared/"]

RUN dotnet restore "backend/StaffService/StaffService.Api/StaffService.Api.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/backend/StaffService/StaffService.Api"
RUN dotnet build "StaffService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "StaffService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN addgroup -g 1000 appuser && adduser -D -u 1000 -G appuser appuser
USER appuser
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

ENTRYPOINT ["dotnet", "StaffService.Api.dll"]
