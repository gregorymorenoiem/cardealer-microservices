# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["SearchService.Api/SearchService.Api.csproj", "SearchService.Api/"]
COPY ["SearchService.Application/SearchService.Application.csproj", "SearchService.Application/"]
COPY ["SearchService.Domain/SearchService.Domain.csproj", "SearchService.Domain/"]
COPY ["SearchService.Infrastructure/SearchService.Infrastructure.csproj", "SearchService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "SearchService.Api/SearchService.Api.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/SearchService.Api"
RUN dotnet build "SearchService.Api.csproj" -c Release -o /app/build
RUN dotnet publish "SearchService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Expose port
EXPOSE 80

# Set entry point
ENTRYPOINT ["dotnet", "SearchService.Api.dll"]
