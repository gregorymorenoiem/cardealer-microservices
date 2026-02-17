FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["ComplianceReportingService.Api/ComplianceReportingService.Api.csproj", "ComplianceReportingService.Api/"]
COPY ["ComplianceReportingService.Application/ComplianceReportingService.Application.csproj", "ComplianceReportingService.Application/"]
COPY ["ComplianceReportingService.Domain/ComplianceReportingService.Domain.csproj", "ComplianceReportingService.Domain/"]
COPY ["ComplianceReportingService.Infrastructure/ComplianceReportingService.Infrastructure.csproj", "ComplianceReportingService.Infrastructure/"]

RUN dotnet restore "ComplianceReportingService.Api/ComplianceReportingService.Api.csproj"

# Copy all source files
COPY . .

WORKDIR "/src/ComplianceReportingService.Api"
RUN dotnet build "ComplianceReportingService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ComplianceReportingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ComplianceReportingService.Api.dll"]
