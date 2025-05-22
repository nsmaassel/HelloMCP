FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["McpServer/McpServer.csproj", "McpServer/"]
RUN dotnet restore "McpServer/McpServer.csproj"

# Copy all files and build
COPY . .
WORKDIR /src/McpServer
RUN dotnet build "McpServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "McpServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Copy published app
COPY --from=publish /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "McpServer.dll"]