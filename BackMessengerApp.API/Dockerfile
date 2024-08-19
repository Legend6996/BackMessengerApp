#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BackMessengerApp.API/BackMessengerApp.API.csproj", "BackMessengerApp.API/"]
COPY ["BackMessengerApp.Application/BackMessengerApp.Application.csproj", "BackMessengerApp.Application/"]
COPY ["BackMessengerApp.Core/BackMessengerApp.Core.csproj", "BackMessengerApp.Core/"]
COPY ["BackMessengerApp.Persistence/BackMessengerApp.Persistence.csproj", "BackMessengerApp.Persistence/"]
RUN dotnet restore "./BackMessengerApp.API/BackMessengerApp.API.csproj"
COPY . .
WORKDIR "/src/BackMessengerApp.API"
RUN dotnet build "./BackMessengerApp.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BackMessengerApp.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BackMessengerApp.API.dll"]