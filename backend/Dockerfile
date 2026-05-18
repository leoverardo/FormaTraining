# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for restore layer caching
COPY ["FitPlatform.Api/FitPlatform.Api.csproj", "FitPlatform.Api/"]
COPY ["FitPlatform.Application/FitPlatform.Application.csproj", "FitPlatform.Application/"]
COPY ["FitPlatform.Domain/FitPlatform.Domain.csproj", "FitPlatform.Domain/"]
COPY ["FitPlatform.Infrastructure/FitPlatform.Infrastructure.csproj", "FitPlatform.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "FitPlatform.Api/FitPlatform.Api.csproj"

# Copy full source
COPY . .

# Publish
WORKDIR /src/FitPlatform.Api
RUN dotnet publish "FitPlatform.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FitPlatform.Api.dll"]
