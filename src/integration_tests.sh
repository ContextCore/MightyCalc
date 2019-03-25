#init database 
cd ./MightyCalc.Reports
dotnet ef database update --startup-project ../MightyCalc.Reports.IntegrationTests/MightyCalc.Reports.IntegrationTests.csproj

#run integration tests for componenets 
cd ../MightyCalc.Reports.IntegrationTests
dotnet test -c Release --no-build

#run integration tests for existing MightyCalc API
cd ../MightyCalc.API.IntegrationTests
dotnet test -c Release --no-build