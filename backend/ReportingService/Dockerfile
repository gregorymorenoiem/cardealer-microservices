FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["ReportingService.Api/ReportingService.Api.csproj", "ReportingService.Api/"]
COPY ["ReportingService.Application/ReportingService.Application.csproj", "ReportingService.Application/"]
COPY ["ReportingService.Domain/ReportingService.Domain.csproj", "ReportingService.Domain/"]
COPY ["ReportingService.Infrastructure/ReportingService.Infrastructure.csproj", "ReportingService.Infrastructure/"]

RUN dotnet restore "ReportingService.Api/ReportingService.Api.csproj"

# Copy all source files
COPY . .

WORKDIR "/src/ReportingService.Api"
RUN dotnet build "ReportingService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReportingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReportingService.Api.dll"]
