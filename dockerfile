FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# CPM: las versiones centrales deben existir ANTES del restore
COPY ["Directory.Packages.props", "./"]

# Proyectos del microservicio
COPY ["src/Eventix.SharedKernel/Eventix.SharedKernel.csproj", "src/Eventix.SharedKernel/"]
COPY ["src/Eventix.EventCatalog/Eventix.EventCatalog.Domain/Eventix.EventCatalog.Domain.csproj", "src/Eventix.EventCatalog/Eventix.EventCatalog.Domain/"]
COPY ["src/Eventix.EventCatalog/Eventix.EventCatalog.Application/Eventix.EventCatalog.Application.csproj", "src/Eventix.EventCatalog/Eventix.EventCatalog.Application/"]
COPY ["src/Eventix.EventCatalog/Eventix.EventCatalog.Infrastructure/Eventix.EventCatalog.Infrastructure.csproj", "src/Eventix.EventCatalog/Eventix.EventCatalog.Infrastructure/"]
COPY ["src/Eventix.EventCatalog/Eventix.EventCatalog.API/Eventix.EventCatalog.API.csproj", "src/Eventix.EventCatalog/Eventix.EventCatalog.API/"]

# Proyectos del Chassis (referenciados por EventCatalog)
COPY ["Eventix.MicroserviceChassis/src/Eventix.Chassis.Core/Eventix.Chassis.Core.csproj", "Eventix.MicroserviceChassis/src/Eventix.Chassis.Core/"]
COPY ["Eventix.MicroserviceChassis/src/Eventix.Chassis.Infrastructure/Eventix.Chassis.Infrastructure.csproj", "Eventix.MicroserviceChassis/src/Eventix.Chassis.Infrastructure/"]
COPY ["Eventix.MicroserviceChassis/src/Eventix.Chassis.Application/Eventix.Chassis.Application.csproj", "Eventix.MicroserviceChassis/src/Eventix.Chassis.Application/"]
COPY ["Eventix.MicroserviceChassis/src/Eventix.Chassis.Api/Eventix.Chassis.Api.csproj", "Eventix.MicroserviceChassis/src/Eventix.Chassis.Api/"]

RUN dotnet restore "src/Eventix.EventCatalog/Eventix.EventCatalog.API/Eventix.EventCatalog.API.csproj"

COPY . .
WORKDIR "/src/src/Eventix.EventCatalog/Eventix.EventCatalog.API"
RUN dotnet publish "Eventix.EventCatalog.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Eventix.EventCatalog.API.dll"]