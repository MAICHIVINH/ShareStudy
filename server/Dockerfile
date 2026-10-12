# Multi-stage Dockerfile for ASP.NET Core 8.0 Web API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/ShareStudy.Api/ShareStudy.Api.csproj", "src/ShareStudy.Api/"]
COPY ["src/ShareStudy.Application/ShareStudy.Application.csproj", "src/ShareStudy.Application/"]
COPY ["src/ShareStudy.Domain/ShareStudy.Domain.csproj", "src/ShareStudy.Domain/"]
COPY ["src/ShareStudy.Infrastructure/ShareStudy.Infrastructure.csproj", "src/ShareStudy.Infrastructure/"]

RUN dotnet restore "src/ShareStudy.Api/ShareStudy.Api.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/ShareStudy.Api"
RUN dotnet build "ShareStudy.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShareStudy.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Render dynamically passes PORT env var (defaults to 8080)
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ShareStudy.Api.dll"]
