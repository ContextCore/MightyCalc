# Generate api classes for C# from OpenAPI specification 
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 as api-gen-env

RUN apt-get update 
RUN apt-get install curl -y
RUN apt-get install zip -y

#download latest Swag toolchain
RUN curl -L http://rsuter.com/Projects/NSwagStudio/archive.php --output ./NSwag.zip
RUN unzip ../NSwag.zip -d NSwag > /dev/null
RUN rm ../NSwag.zip > /dev/null

COPY . ./MightyCalc.API
WORKDIR MightyCalc.API

#Generating API controller
RUN dotnet ../NSwag/NetCore22/dotnet-nswag.dll swagger2cscontroller /input:"./MightyCalcAPI.yaml" /classname:Api /namespace:MightyCalc.API /UseLiquidTemplates:true /ControllerBaseClass:Microsoft.AspNetCore.Mvc.ControllerBase /AspNetNamespace:"Microsoft.AspNetCore.Mvc" /output:"/MightyCalc.API/MightyCalc.API/Controllers/MightyCalcController.cs" /ResponseArrayType:"System.Collections.Generic.IReadOnlyCollection"  /ArrayBaseType:"System.Collections.Generic.IReadonlyCollection" /ArrayInstanceType:"System.Collections.Generic.List"

#Generating API client in C#
RUN dotnet ../NSwag/NetCore22/dotnet-nswag.dll swagger2csclient /input:"./MightyCalcAPI.yaml" /classname:MightyCalcClient /namespace:MightyCalc.Client /UseLiquidTemplates:true /output:"/MightyCalc.API/MightyCalc.Client/Client.cs" /GenerateClientInterfaces:true /InjectHttpClient:true /generateDataAnnotations:false /exceptionClass:MightyCalcException /generateOptionalParameters:true /ArrayBaseType:"System.Collections.Generic.IReadonlyCollection" /responseArrayType:"System.Collections.Generic.IReadOnlyCollection" /arrayType:"System.Collections.Generic.IReadOnlyCollection"
#build the project and get binaries for production 
FROM microsoft/dotnet:3.0-sdk as build-env

COPY --from=api-gen-env ./MightyCalc.API ./MightyCalc.API
WORKDIR MightyCalc.API

RUN dotnet build -c Release -v minimal
RUN mkdir -p /swagger
RUN cp ./MightyCalcAPI.yaml /swagger/MightyCalcAPI.yaml #for tests
RUN dotnet publish -c Release -o publish --no-build