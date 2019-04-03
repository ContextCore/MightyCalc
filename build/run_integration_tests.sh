

ls

 exit 0 

./deploy/create_environment.sh dd

ls 
exit 1 

testPlan="${1:-all}" #api , persistence, all
echo "Test plan is $testPlan"
#init database 
#cd ./MightyCalc.Reports
#dotnet ef database update --startup-project ../MightyCalc.Reports.IntegrationTests/MightyCalc.Reports.IntegrationTests.csproj

if [ $testPlan == "persistence" ] || [ $testPlan == "all" ]; then
echo "Executing persistence tests"
#run integration tests for componenets 
#needs Database available
cd ./src/MightyCalc.Reports.IntegrationTests
dotnet test -c Release --no-build

#run integration tests for componenets 
#needs Database available
cd ../MightyCalc.Node.IntegrationTests
dotnet test -c Release --no-build
fi

if [ $testPlan == "api" ] || [ $testPlan == "all" ]; then
echo "Executing api tests"
#run integration tests for existing MightyCalc API
#needs API available as well as Database
cd ../MightyCalc.API.IntegrationTests
dotnet test -c Release --no-build
fi