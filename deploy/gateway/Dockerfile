#FROM mcr.microsoft.com/dotnet/aspnet:8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809
WORKDIR /app
COPY app/ .
ENTRYPOINT ["dotnet", "Gateway.Api.dll"]
