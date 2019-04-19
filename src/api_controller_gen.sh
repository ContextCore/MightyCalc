pathToNSwag=$1
outputFile=$2

dotnet $pathToNSwag swagger2cscontroller /input:"./MightyCalcAPI.yaml" /classname:Api /namespace:MightyCalc.API /UseLiquidTemplates:true /ControllerBaseClass:Microsoft.AspNetCore.Mvc.ControllerBase /AspNetNamespace:"Microsoft.AspNetCore.Mvc" /output:"$outputFile" /ResponseArrayType:"System.Collections.Generic.IReadOnlyCollection"  /ArrayBaseType:"System.Collections.Generic.IReadonlyCollection" /ArrayInstanceType:"System.Collections.Generic.List"