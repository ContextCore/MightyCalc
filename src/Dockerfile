#Prepare image with .Net Core 2.2 and 3.0 runtime for later use
FROM microsoft/dotnet:3.0-runtime as dotnet-23-runtime
RUN curl -L https://dot.net/v1/dotnet-install.sh --output dotnet-install.sh
RUN chmod +x ./dotnet-install.sh
RUN ./dotnet-install.sh -v 2.1.9 --runtime dotnet -i /usr/share/dotnet

FROM microsoft/dotnet:3.0-aspnetcore-runtime  as aspnetcore-23-runtime
RUN curl -L https://dot.net/v1/dotnet-install.sh --output dotnet-install.sh
RUN chmod +x ./dotnet-install.sh
RUN ./dotnet-install.sh -v 2.1.9 --runtime dotnet -i /usr/share/dotnet
RUN ./dotnet-install.sh -v 2.1.9 --runtime aspnetcore -i /usr/share/dotnet

# Generate api classes for C# from OpenAPI specification 
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 as api-gen-env
RUN apt-get update 
RUN apt-get install curl -y
RUN apt-get install zip -y
RUN mkdir /usr/bin/MightyCalc 
WORKDIR /usr/bin/MightyCalc
#download latest Swag toolchain
RUN curl -L http://rsuter.com/Projects/NSwagStudio/archive.php --output ./NSwag.zip
RUN unzip ./NSwag.zip -d NSwag > /dev/null
RUN rm ./NSwag.zip > /dev/null

COPY . .

RUN ./api_controller_gen.sh "./NSwag/NetCore22/dotnet-nswag.dll" "./MightyCalc.API/MightyCalc.API/Controllers/MightyCalcController.cs"
RUN ./api_client_gen.sh  "./NSwag/NetCore22/dotnet-nswag.dll" "./MightyCalc.API/MightyCalc.Client/Client.cs"

#Generating API controller
#RUN dotnet ./NSwag/NetCore22/dotnet-nswag.dll swagger2cscontroller /input:"./MightyCalcAPI.yaml" /classname:Api /namespace:MightyCalc.API /UseLiquidTemplates:true /ControllerBaseClass:Microsoft.AspNetCore.Mvc.ControllerBase /AspNetNamespace:"Microsoft.AspNetCore.Mvc" /output:"/MightyCalc.API/MightyCalc.API/Controllers/MightyCalcController.cs" /ResponseArrayType:"System.Collections.Generic.IReadOnlyCollection"  /ArrayBaseType:"System.Collections.Generic.IReadonlyCollection" /ArrayInstanceType:"System.Collections.Generic.List"
#Generating API client in C#
#RUN dotnet ./NSwag/NetCore22/dotnet-nswag.dll swagger2csclient /input:"./MightyCalcAPI.yaml" /classname:MightyCalcClient /namespace:MightyCalc.Client /UseLiquidTemplates:true /output:"/MightyCalc.API/MightyCalc.Client/Client.cs" /GenerateClientInterfaces:true /InjectHttpClient:true /generateDataAnnotations:false /exceptionClass:MightyCalcException /generateOptionalParameters:true /ArrayBaseType:"System.Collections.Generic.IReadonlyCollection" /responseArrayType:"System.Collections.Generic.IReadOnlyCollection" /arrayType:"System.Collections.Generic.IReadOnlyCollection"
RUN rm -rf ./NSwag

#build the project and get binaries for production 
FROM microsoft/dotnet:3.0-sdk as build-env
# Install Petabridge.Cmd client
RUN dotnet tool install --global pbm 

# Needed because https://stackoverflow.com/questions/51977474/install-dotnet-core-tool-dockerfile


COPY --from=api-gen-env /usr/bin/MightyCalc /usr/bin/MightyCalc
WORKDIR /usr/bin/MightyCalc
RUN dotnet --list-runtimes
RUN dotnet build -c Release -v quiet
RUN mkdir -p /swagger
RUN cp ./MightyCalcAPI.yaml /swagger/MightyCalcAPI.yaml #for tests
ENTRYPOINT [ "dotnet" ]

FROM build-env as api-publish-env
RUN dotnet publish ./MightyCalc.API/MightyCalc.API.csproj -c Release --no-build -o publish -v quiet

FROM build-env as node-publish-env
RUN dotnet publish ./MightyCalc.NodeHost/MightyCalc.NodeHost.csproj -c Release --no-build -o publish -v quiet

FROM build-env as lighthouse-publish-env
RUN dotnet publish ./MightyCalc.LightHouse/MightyCalc.LightHouse.csproj -c Release --no-build -o publish -v quiet

FROM aspnetcore-23-runtime as api-runtime
ENV PATH="${PATH}:/root/.dotnet/tools"
COPY --from=build-env /root/.dotnet/tools /root/.dotnet/tools
COPY --from=api-publish-env /usr/bin/MightyCalc/publish /usr/bin/MightyCalc  
WORKDIR /usr/bin/MightyCalc
EXPOSE 80
EXPOSE 9110
ENV MightyCalc_NodePort 2555
ENTRYPOINT ["dotnet"]
CMD ["MightyCalc.API.dll"]

FROM dotnet-23-runtime as node-runtime
ENV PATH="${PATH}:/root/.dotnet/tools"
COPY --from=build-env /root/.dotnet/tools /root/.dotnet/tools 
COPY --from=node-publish-env /usr/bin/MightyCalc/publish /usr/bin/MightyCalc
WORKDIR /usr/bin/MightyCalc
EXPOSE 2555
EXPOSE 9110
ENV MightyCalc_NodePort 2555
ENTRYPOINT ["dotnet"]
CMD ["MightyCalc.NodeHost.dll"]

FROM dotnet-23-runtime as lighthouse-runtime
ENV PATH="${PATH}:/root/.dotnet/tools"
COPY --from=build-env /root/.dotnet/tools /root/.dotnet/tools 
COPY --from=lighthouse-publish-env /usr/bin/MightyCalc/publish /usr/bin/MightyCalc  
WORKDIR /usr/bin/MightyCalc
EXPOSE 2555
EXPOSE 9110
ENV MightyCalc_NodePort 2555
ENTRYPOINT ["dotnet"]
CMD ["MightyCalc.LightHouse.dll"]