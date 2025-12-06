FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AppointmentService/AppointmentService.Api/AppointmentService.Api.csproj", "AppointmentService/AppointmentService.Api/"]
COPY ["AppointmentService/AppointmentService.Application/AppointmentService.Application.csproj", "AppointmentService/AppointmentService.Application/"]
COPY ["AppointmentService/AppointmentService.Domain/AppointmentService.Domain.csproj", "AppointmentService/AppointmentService.Domain/"]
COPY ["AppointmentService/AppointmentService.Infrastructure/AppointmentService.Infrastructure.csproj", "AppointmentService/AppointmentService.Infrastructure/"]
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
RUN dotnet restore "AppointmentService/AppointmentService.Api/AppointmentService.Api.csproj"
COPY . .
WORKDIR "/src/AppointmentService/AppointmentService.Api"
RUN dotnet build "AppointmentService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AppointmentService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppointmentService.Api.dll"]
