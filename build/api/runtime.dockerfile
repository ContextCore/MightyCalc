#create production-ready container
#want to use different image, but swagger UI has compatibility issues with .net core preview3 version
FROM aleskov/mightycalc-buildenv as integration-env
FROM microsoft/dotnet:3.0-aspnetcore-runtime as runtime
COPY --from=integration-env /MightyCalc/publish /MightyCalc   
WORKDIR /MightyCalc
EXPOSE 80
ENTRYPOINT ["dotnet", "MightyCalc.API.dll"]
