# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project file
COPY ["KeycloakAuth.csproj", "./"]

# Restore dependencies
RUN dotnet restore "KeycloakAuth.csproj"

# Copy source code
COPY . .

# Build the application
RUN dotnet build "KeycloakAuth.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish

RUN dotnet publish "KeycloakAuth.csproj" -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8000/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "KeycloakAuth.dll"]
