dotnet build -c Release src/OneBRC/OneBRC.csproj;
dotnet run --project src/OneBRC/OneBRC.csproj -c Release --no-build -- naive .\measurements-20.txt;