# Stage 1: Build and Test
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all .sln, .csproj, and other project files to leverage Docker layer caching
COPY ["TravelBookingPlatform.sln", "./"]
COPY ["Directory.Build.props", "./"]

# --- Source Projects ---
COPY ["src/TravelBookingPlatform.Core/TravelBookingPlatform.Core.Domain/TravelBookingPlatform.Core.Domain.csproj", "src/TravelBookingPlatform.Core/TravelBookingPlatform.Core.Domain/"]
COPY ["src/TravelBookingPlatform.Core/TravelBookingPlatform.Core.Application/TravelBookingPlatform.Core.Application.csproj", "src/TravelBookingPlatform.Core/TravelBookingPlatform.Core.Application/"]
COPY ["src/TravelBookingPlatform.Modules.Identity/TravelBookingPlatform.Modules.Identity.Domain/TravelBookingPlatform.Modules.Identity.Domain.csproj", "src/TravelBookingPlatform.Modules.Identity/TravelBookingPlatform.Modules.Identity.Domain/"]
COPY ["src/TravelBookingPlatform.Modules.Identity/TravelBookingPlatform.Modules.Identity.Application/TravelBookingPlatform.Modules.Identity.Application.csproj", "src/TravelBookingPlatform.Modules.Identity/TravelBookingPlatform.Modules.Identity.Application/"]
COPY ["src/TravelBookingPlatform.Modules.Identity/TravelBookingPlatform.Modules.Identity.Infrastructure/TravelBookingPlatform.Modules.Identity.Infrastructure.csproj", "src/TravelBookingPlatform.Modules.Identity/TravelBookingPlatform.Modules.Identity.Infrastructure/"]
COPY ["src/TravelBookingPlatform.Modules.Identity/TravelBookingPlatform.Modules.Identity.Api/TravelBookingPlatform.Modules.Identity.Api.csproj", "src/TravelBookingPlatform.Modules.Identity/TravelBookingPlatform.Modules.Identity.Api/"]
COPY ["src/TravelBookingPlatform.Modules.Hotels/TravelBookingPlatform.Modules.Hotels.Domain/TravelBookingPlatform.Modules.Hotels.Domain.csproj", "src/TravelBookingPlatform.Modules.Hotels/TravelBookingPlatform.Modules.Hotels.Domain/"]
COPY ["src/TravelBookingPlatform.Modules.Hotels/TravelBookingPlatform.Modules.Hotels.Application/TravelBookingPlatform.Modules.Hotels.Application.csproj", "src/TravelBookingPlatform.Modules.Hotels/TravelBookingPlatform.Modules.Hotels.Application/"]
COPY ["src/TravelBookingPlatform.Modules.Hotels/TravelBookingPlatform.Modules.Hotels.Infrastructure/TravelBookingPlatform.Modules.Hotels.Infrastructure.csproj", "src/TravelBookingPlatform.Modules.Hotels/TravelBookingPlatform.Modules.Hotels.Infrastructure/"]
COPY ["src/TravelBookingPlatform.Modules.Hotels/TravelBookingPlatform.Modules.Hotels.Api/TravelBookingPlatform.Modules.Hotels.Api.csproj", "src/TravelBookingPlatform.Modules.Hotels/TravelBookingPlatform.Modules.Hotels.Api/"]
COPY ["src/TravelBookingPlatform.SharedInfrastructure/TravelBookingPlatform.SharedInfrastructure.csproj", "src/TravelBookingPlatform.SharedInfrastructure/"]
COPY ["src/TravelBookingPlatform.Host/TravelBookingPlatform.Host.csproj", "src/TravelBookingPlatform.Host/"]

# --- Test Projects ---
COPY ["tests/TravelBookingPlatform.UnitTests/TravelBookingPlatform.Core.UnitTests/TravelBookingPlatform.Core.UnitTests.csproj", "tests/TravelBookingPlatform.UnitTests/TravelBookingPlatform.Core.UnitTests/"]
COPY ["tests/TravelBookingPlatform.UnitTests/TravelBookingPlatform.Identity.UnitTests/TravelBookingPlatform.Identity.UnitTests.csproj", "tests/TravelBookingPlatform.UnitTests/TravelBookingPlatform.Identity.UnitTests/"]
COPY ["tests/TravelBookingPlatform.UnitTests/TravelBookingPlatform.Hotels.UnitTests/TravelBookingPlatform.Hotels.UnitTests.csproj", "tests/TravelBookingPlatform.UnitTests/TravelBookingPlatform.Hotels.UnitTests/"]
COPY ["tests/TravelBookingPlatform.IntegrationTests/TravelBookingPlatform.Host.IntegrationTests/TravelBookingPlatform.Host.IntegrationTests.csproj", "tests/TravelBookingPlatform.IntegrationTests/TravelBookingPlatform.Host.IntegrationTests/"]
COPY ["tests/TravelBookingPlatform.IntegrationTests/TravelBookingPlatform.Modules.IntegrationTests/TravelBookingPlatform.Modules.IntegrationTests.csproj", "tests/TravelBookingPlatform.IntegrationTests/TravelBookingPlatform.Modules.IntegrationTests/"]


# Restore dependencies for all projects in the solution. This is a separate layer.
RUN dotnet restore "TravelBookingPlatform.sln"

# Copy the entire source code and test code. This is done after restore to cache layers better.
COPY . .

# Publish the Host project. Remove --no-restore to make this command more robust.
RUN dotnet publish "src/TravelBookingPlatform.Host/TravelBookingPlatform.Host.csproj" -c Release -o /app/publish


# Stage 2: Create the final lightweight runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port the app will run on
EXPOSE 8080
EXPOSE 8081

# Define the entry point for the container
ENTRYPOINT ["dotnet", "TravelBookingPlatform.Host.dll"]