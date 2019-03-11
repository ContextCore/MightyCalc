# Generate api classes for C# from OpenAPI specification 
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 as api-gen-env

RUN apt-get update 
RUN apt-get install curl -y
RUN apt-get install zip -y

#download latest Swag toolchain
RUN curl -L http://rsuter.com/Projects/NSwagStudio/archive.php --output ./NSwag.zip
RUN unzip ../NSwag.zip -d NSwag
RUN rm ../NSwag.zip

COPY . ./MightyCalc.API
WORKDIR MightyCalc.API

RUN dotnet ../NSwag/NetCore22/dotnet-nswag.dll swagger2cscontroller /input:"./MightyCalcAPI.yml" /classname:Api /namespace:MightyCalc.API /UseLiquidTemplates:true /ControllerBaseClass:Microsoft.AspNetCore.Mvc.ControllerBase /AspNetNamespace:"Microsoft.AspNetCore.Mvc" /output:"/MightyCalc.API/MightyCalc.API/Controllers/MightyCalcController.cs" /ResponseArrayType:"System.Collections.Generic.IReadOnlyCollection"  /ArrayBaseType:"System.Collections.Generic.IReadonlyCollection" /ArrayInstanceType:"System.Collections.Generic.List"

#build the project and get binaries for production 
FROM microsoft/dotnet:3.0-sdk as build-env

COPY --from=api-gen-env ./MightyCalc.API ./MightyCalc.API
WORKDIR MightyCalc.API

RUN dotnet publish -c Release -o publish
RUN ls

#create production-ready container
FROM microsoft/dotnet:3.0-sdk as runtime
RUN ls
COPY --from=build-env /MightyCalc.API/publish /MightyCalc.API   
WORKDIR /MightyCalc.API
RUN ls
EXPOSE 80
ENTRYPOINT ["dotnet", "MightyCalc.API.dll"]


