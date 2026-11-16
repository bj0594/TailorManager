# Builds the frontend image. Build from the repository root:
# docker build -t tailormanager-web -f TailorManager.Web/Dockerfile .

# Build stage: the SDK image compiles and publishes the frontend.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish TailorManager.Web/TailorManager.Web.csproj -c Release -o /app/publish

# Runtime stage: a smaller image that only runs the published frontend on port 8080.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TailorManager.Web.dll"]
