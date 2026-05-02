FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AdegaRoyal.Api.csproj", "./"] 
RUN dotnet restore "AdegaRoyal.Api.csproj"

COPY . .
RUN dotnet publish "AdegaRoyal.Api.csproj" -c Release -o /app/publish -r linux-x64 --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN apt-get update && apt-get install -y libicu-dev curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "AdegaRoyal.Api.dll"]