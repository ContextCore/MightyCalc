pathToNSwag=$1
outputFile=$2

dotnet $pathToNSwag swagger2csclient /input:"./MightyCalcAPI.yaml" /classname:MightyCalcClient /namespace:MightyCalc.Client /UseLiquidTemplates:true /output:"$outputFile" /GenerateClientInterfaces:true /InjectHttpClient:true /generateDataAnnotations:false /exceptionClass:MightyCalcException /generateOptionalParameters:true /ArrayBaseType:"System.Collections.Generic.IReadonlyCollection" /responseArrayType:"System.Collections.Generic.IReadOnlyCollection" /arrayType:"System.Collections.Generic.IReadOnlyCollection"
