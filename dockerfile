# Imagen base para ejecutar .NET 9
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Dependencias para generación de PDF
RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates \
    fontconfig \
    fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

# Imagen para compilar
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ClinicaAmelita.Api.csproj ./
RUN dotnet restore ClinicaAmelita.Api.csproj

COPY . .
RUN dotnet publish ClinicaAmelita.Api.csproj -c Release -o /app/publish --no-restore

# Imagen final
FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "ClinicaAmelita.Api.dll"]
