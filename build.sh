# Add dotnet build script and add a test script
# To run it open Linux terminal and type `sh build.sh || bash build.sh`

# //   _________
# //  /   _____/__.__._______  ____   ____  
# //  \_____  <   |  |\_  __ \/  _ \ /    \ 
# //  /        \___  | |  | \(  <_> )   |  \
# // /_______  / ____| |__|   \____/|___|  /
# //         \/\/               


dotnet build ./src/syron.sln /nologo

dotnet test ./src/Syron.Tests/Syron.Tests.csproj