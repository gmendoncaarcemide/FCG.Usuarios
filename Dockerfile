# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia somente o necessário do monorepo
COPY . ./FCG.Usuarios/

# Restaura dependências e publica
RUN dotnet restore "FCG.Usuarios/FCG.Usuarios.API/FCG.Usuarios.API.csproj"
RUN dotnet publish "FCG.Usuarios/FCG.Usuarios.API/FCG.Usuarios.API.csproj" -c Release -o /app/publish

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCG.Usuarios.API.dll"]
