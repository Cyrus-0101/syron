# Add dotnet build script
# To run it open Linux terminal and type `sh build.sh || bash build.sh`

dotnet build ./src/Syron.sln /nologo

dotnet test ./src/Syron.Tests/Syron.Tests.csproj