# syntax=docker/dockerfile:1

# Stage 1: Build the application
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

# Set up working directory
WORKDIR /source

# Copy project files
COPY ["WebEngineering.csproj", "WebEngineering/"]


# Restore dependencies based on architecture to cache packages per platform
ARG TARGETARCH
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore "WebEngineering.csproj" --arch ${TARGETARCH/amd64/x64}

# Copy the entire source
COPY . .

# Set the working directory to the project folder
WORKDIR /source/WebEngineering

# Build and publish the application
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --arch ${TARGETARCH/amd64/x64}

################################################################################
# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

# Set up working directory
WORKDIR /app

# Copy published application from the build stage
COPY --from=build /app/publish .

# Set up necessary permissions and expose port
USER root
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "WebEngineering.dll"]