# Comment
FROM microsoft/dotnet:sdk as build-env

RUN apt-get update 
RUN apt-get install curl -y
RUN apt-get install zip -y
#download latest Swag toolchain
RUN curl -L http://rsuter.com/Projects/NSwagStudio/archive.php --output ./NSwag.zip

#unzip it 
RUN unzip ../NSwag.zip -d NSwag
RUN rm ../NSwag.zip

COPY . ./MightyCalc.API
WORKDIR MightyCalc.API
RUN ls
RUN dotnet ../NSwag/NetCore22/dotnet-nswag.dll swagger2cscontroller /input:"./MightyCalcAPI.yml" /classname:ApiController /namespace:MightyCalc.API /UseLiquidTemplates:true /ControllerBaseClass:Microsoft.AspNetCore.Mvc.ControllerBase /AspNetNamespace:"Microsoft.AspNetCore.Mvc" /output:"/MightyCalc.API/MightyCalc.API/Controllers/MightyCalcController.cs" /ResponseArrayType:"System.Collections.Generic.List" /ArrayBaseType:"System.Collections.Generic.List" /ArrayInstanceType:"System.Collections.Generic.List"
RUN ls
RUN dotnet build 

ENTRYPOINT ["dotnet"]

CMD ["MightyCalc.dll"]




