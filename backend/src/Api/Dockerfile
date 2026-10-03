# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for caching
COPY ["Crm.sln", "./"]
COPY ["src/Crm.Domain/Crm.Domain.csproj", "src/Crm.Domain/"]
COPY ["src/Crm.Application/Crm.Application.csproj", "src/Crm.Application/"]
COPY ["src/Crm.Infrastructure/Crm.Infrastructure.csproj", "src/Crm.Infrastructure/"]
COPY ["src/Crm.Api/Crm.Api.csproj", "src/Crm.Api/"]

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build and Publish
WORKDIR "/src/src/Crm.Api"
# Note: For Native AOT, add -a options. Here we use standard sleek publish.
RUN dotnet publish "Crm.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

# Security: Run as non-root user
USER app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Crm.Api.dll"]
