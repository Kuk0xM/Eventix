# ============================================================
# STAGE 1: BUILD
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Eventix.SharedKernel/Eventix.SharedKernel.csproj", "src/Eventix.SharedKernel/"]
COPY ["src/Eventix.EventCatalog/Eventix.EventCatalog.Domain/Eventix.EventCatalog.Domain.csproj", "src/Eventix.EventCatalog/Eventix.EventCatalog.Domain/"]
COPY ["src/Eventix.EventCatalog/Eventix.EventCatalog.Application/Eventix.EventCatalog.Application.csproj", "src/Eventix.EventCatalog/Eventix.EventCatalog.Application/"]
COPY ["src/Eventix.EventCatalog/Eventix.EventCatalog.Infrastructure/Eventix.EventCatalog.Infrastructure.csproj", "src/Eventix.EventCatalog/Eventix.EventCatalog.Infrastructure/"]
COPY ["src/Eventix.EventCatalog/Eventix.EventCatalog.API/Eventix.EventCatalog.API.csproj", "src/Eventix.EventCatalog/Eventix.EventCatalog.API/"]

RUN dotnet restore "src/Eventix.EventCatalog/Eventix.EventCatalog.API/Eventix.EventCatalog.API.csproj"

COPY . .
WORKDIR "/src/src/Eventix.EventCatalog/Eventix.EventCatalog.API"
RUN dotnet build "Eventix.EventCatalog.API.csproj" -c Release -o /app/build

# ============================================================
# STAGE 2: PUBLISH
# ============================================================
FROM build AS publish
RUN dotnet publish "Eventix.EventCatalog.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================================================
# STAGE 3: RUNTIME
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# ✅ NUEVO: Instalar librerías de Kerberos para eliminar el warning de Npgsql
# y otras librerías comunes que los drivers de DB necesitan
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        libgssapi-krb5-2 \
        libicu-dev \
        tzdata \
    && rm -rf /var/lib/apt/lists/*

# Security: run as non-root user
USER $APP_UID

EXPOSE 8080
ENTRYPOINT ["dotnet", "Eventix.EventCatalog.API.dll"]