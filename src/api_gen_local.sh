
nswagPath="../../NSwag/NetCore22/dotnet-nswag.dll"
controllerFile="./MightyCalc.API/Controllers/MightyCalcController.cs"
./api_controller_gen.sh  $nswagPath $controllerFile
clientFile="./MightyCalc.Client/Client.cs"
./api_client_gen.sh  $nswagPath $clientFile
