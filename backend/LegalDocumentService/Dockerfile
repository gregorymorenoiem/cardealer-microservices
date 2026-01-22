FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LegalDocumentService.Api/LegalDocumentService.Api.csproj", "LegalDocumentService.Api/"]
COPY ["LegalDocumentService.Application/LegalDocumentService.Application.csproj", "LegalDocumentService.Application/"]
COPY ["LegalDocumentService.Domain/LegalDocumentService.Domain.csproj", "LegalDocumentService.Domain/"]
COPY ["LegalDocumentService.Infrastructure/LegalDocumentService.Infrastructure.csproj", "LegalDocumentService.Infrastructure/"]
RUN dotnet restore "LegalDocumentService.Api/LegalDocumentService.Api.csproj"
COPY . .
WORKDIR "/src/LegalDocumentService.Api"
RUN dotnet build "LegalDocumentService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LegalDocumentService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LegalDocumentService.Api.dll"]
