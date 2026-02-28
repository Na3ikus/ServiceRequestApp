# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ServiceDeskSystem.Api/ServiceDeskSystem.Api.csproj", "ServiceDeskSystem.Api/"]
COPY ["ServiceDeskSystem.Application/ServiceDeskSystem.Application.csproj", "ServiceDeskSystem.Application/"]
COPY ["ServiceDeskSystem.Domain/ServiceDeskSystem.Domain.csproj", "ServiceDeskSystem.Domain/"]
COPY ["ServiceDeskSystem.Infrastructure/ServiceDeskSystem.Infrastructure.csproj", "ServiceDeskSystem.Infrastructure/"]
RUN dotnet restore "./ServiceDeskSystem.Api/ServiceDeskSystem.Api.csproj"
COPY . .
WORKDIR "/src/ServiceDeskSystem.Api"
RUN dotnet build "./ServiceDeskSystem.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ServiceDeskSystem.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServiceDeskSystem.Api.dll"]
