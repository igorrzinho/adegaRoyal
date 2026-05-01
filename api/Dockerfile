# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["*.csproj", "./"]
RUN dotnet restore

# Copy the remaining source code
COPY . .

# Build and publish the application, setting the assembly name to AdegaRoyal.Api as requested
RUN dotnet publish -c Release -o /app/publish /p:AssemblyName=AdegaRoyal.Api

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install libicu-dev to support SQL Server globalization
RUN apt-get update \
    && apt-get install -y libicu-dev \
    && rm -rf /var/lib/apt/lists/*

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose port 8080 (the default for .NET 8+ containers, and we map it in compose)
EXPOSE 8080

# Configure the entrypoint to the expected DLL name
ENTRYPOINT ["dotnet", "AdegaRoyal.Api.dll"]
