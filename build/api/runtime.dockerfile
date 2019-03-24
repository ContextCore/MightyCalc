#create production-ready container
FROM aleskov/mightycalc-buildenv as publish-env
WORKDIR /usr/bin/MightyCalc
RUN ls
RUN dotnet publish ./MightyCalc.API/MightyCalc.API.csproj -c Release --no-build -o publish
FROM microsoft/dotnet:3.0-aspnetcore-runtime as runtime
COPY --from=publish-env /usr/bin/MightyCalc/publish /usr/bin/MightyCalc  
WORKDIR /usr/bin/MightyCalc
EXPOSE 80
ENTRYPOINT ["dotnet", "MightyCalc.API.dll"]
