FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["CRMService/CRMService.Api/CRMService.Api.csproj", "CRMService/CRMService.Api/"]
COPY ["CRMService/CRMService.Application/CRMService.Application.csproj", "CRMService/CRMService.Application/"]
COPY ["CRMService/CRMService.Domain/CRMService.Domain.csproj", "CRMService/CRMService.Domain/"]
COPY ["CRMService/CRMService.Infrastructure/CRMService.Infrastructure.csproj", "CRMService/CRMService.Infrastructure/"]
COPY ["SharedKernel/SharedKernel.csproj", "SharedKernel/"]

# Restore dependencies
RUN dotnet restore "CRMService/CRMService.Api/CRMService.Api.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/CRMService/CRMService.Api"
RUN dotnet build "CRMService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CRMService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CRMService.Api.dll"]
