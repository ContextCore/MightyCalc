set -euo pipefail

echo "creating test pod" 
kubectl apply -f mightycalc-tests.yaml

./wait_pod_is_ready.sh 60 1

echo "Starting node integration tests"
kubectl exec -it mightycalc-test -- dotnet test ./MightyCalc.Node.IntegrationTests/MightyCalc.Node.IntegrationTests.csproj -c Release --no-build > node_integration_tests.txt

echo "Starting report integration tests"
kubectl exec -it mightycalc-test -- dotnet test ./MightyCalc.Reports.IntegrationTests/MightyCalc.Reports.IntegrationTests.csproj -c Release --no-build > report_integration_tests.txt

echo "Starting api integration tests"
kubectl exec -it mightycalc-test -- dotnet test ./MightyCalc.API.IntegrationTests/MightyCalc.API.IntegrationTests.csproj -c Release --no-build > api_integration_tests.txt
