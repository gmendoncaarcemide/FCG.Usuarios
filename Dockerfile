# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos do projeto
COPY ["FCG.Usuarios.API/FCG.Usuarios.API.csproj", "FCG.Usuarios.API/"]
COPY ["FCG.Usuarios.Application/FCG.Usuarios.Application.csproj", "FCG.Usuarios.Application/"]
COPY ["FCG.Usuarios.Domain/FCG.Usuarios.Domain.csproj", "FCG.Usuarios.Domain/"]
COPY ["FCG.Usuarios.Infrastructure/FCG.Usuarios.Infrastructure.csproj", "FCG.Usuarios.Infrastructure/"]

# Restaura dependências
RUN dotnet restore "FCG.Usuarios.API/FCG.Usuarios.API.csproj"

# Copia tudo e compila
COPY . .
WORKDIR "/src/FCG.Usuarios.API"
RUN dotnet publish "FCG.Usuarios.API.csproj" -c Release -o /app/publish

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCG.Usuarios.API.dll"]
