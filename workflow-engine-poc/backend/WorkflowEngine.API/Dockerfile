FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY WorkflowEngine.Core/WorkflowEngine.Core.csproj WorkflowEngine.Core/
COPY WorkflowEngine.Data/WorkflowEngine.Data.csproj WorkflowEngine.Data/
COPY WorkflowEngine.API/WorkflowEngine.API.csproj WorkflowEngine.API/

# Restore dependencies
RUN dotnet restore WorkflowEngine.API/WorkflowEngine.API.csproj

# Copy source code
COPY . .

# Build the application
RUN dotnet build WorkflowEngine.API/WorkflowEngine.API.csproj -c Release -o /app/build

# Publish the application
RUN dotnet publish WorkflowEngine.API/WorkflowEngine.API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "WorkflowEngine.API.dll"]
