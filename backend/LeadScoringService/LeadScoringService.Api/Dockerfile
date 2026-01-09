# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["LeadScoringService.Api/LeadScoringService.Api.csproj", "LeadScoringService.Api/"]
COPY ["LeadScoringService.Application/LeadScoringService.Application.csproj", "LeadScoringService.Application/"]
COPY ["LeadScoringService.Domain/LeadScoringService.Domain.csproj", "LeadScoringService.Domain/"]
COPY ["LeadScoringService.Infrastructure/LeadScoringService.Infrastructure.csproj", "LeadScoringService.Infrastructure/"]

RUN dotnet restore "LeadScoringService.Api/LeadScoringService.Api.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/LeadScoringService.Api"
RUN dotnet build "LeadScoringService.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "LeadScoringService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "LeadScoringService.Api.dll"]
