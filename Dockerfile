# Build: usar SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar solo el proyecto y restaurar dependencias
COPY ./ProyectoSeguridadInformatica/*.csproj ./
RUN dotnet restore

# Copiar el resto del código y publicar
COPY ./ProyectoSeguridadInformatica/. ./
RUN dotnet publish -c Release -o /app/publish

# Runtime: usar imagen más ligera para ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copiar archivos publicados desde la etapa de build
COPY --from=build /app/publish .

# Exponer el puerto que tu app utiliza (opcional, por ejemplo 5000)
EXPOSE 5000

# Ejecutar la aplicación
ENTRYPOINT ["dotnet", "ProyectoSeguridadInformatica.dll"]
