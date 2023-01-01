FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore test project
RUN dotnet restore ./AdvanceFeatureFilter.Tests/AdvanceFeatureFilter.Tests.csproj
# Run test - just
RUN dotnet build ./AdvanceFeatureFilter.Tests/AdvanceFeatureFilter.Tests.csproj

ENTRYPOINT ["dotnet", "test", "./AdvanceFeatureFilter.Tests/AdvanceFeatureFilter.Tests.csproj", "--no-restore"]