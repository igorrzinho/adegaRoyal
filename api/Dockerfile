FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["KeycloakAuth.csproj", "./"]
RUN dotnet restore "KeycloakAuth.csproj"

COPY . .
RUN dotnet publish "KeycloakAuth.csproj" -c Release -o /app/publish -r linux-x64 --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN apt-get update && apt-get install -y \
    libicu-dev \
    curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:10000/health || exit 1

ENTRYPOINT ["dotnet", "KeycloakAuth.dll"]