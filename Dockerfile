FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/AltensorGlobalHealthCare.Domain/AltensorGlobalHealthCare.Domain.csproj", "src/AltensorGlobalHealthCare.Domain/"]
COPY ["src/AltensorGlobalHealthCare.Application/AltensorGlobalHealthCare.Application.csproj", "src/AltensorGlobalHealthCare.Application/"]
COPY ["src/AltensorGlobalHealthCare.Persistence/AltensorGlobalHealthCare.Persistence.csproj", "src/AltensorGlobalHealthCare.Persistence/"]
COPY ["src/AltensorGlobalHealthCare.Infrastructure/AltensorGlobalHealthCare.Infrastructure.csproj", "src/AltensorGlobalHealthCare.Infrastructure/"]
COPY ["src/AltensorGlobalHealthCare.WebApi/AltensorGlobalHealthCare.WebApi.csproj", "src/AltensorGlobalHealthCare.WebApi/"]

RUN dotnet restore "src/AltensorGlobalHealthCare.WebApi/AltensorGlobalHealthCare.WebApi.csproj"

COPY . .
WORKDIR "/src/src/AltensorGlobalHealthCare.WebApi"
RUN dotnet build "AltensorGlobalHealthCare.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AltensorGlobalHealthCare.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AltensorGlobalHealthCare.WebApi.dll"]

