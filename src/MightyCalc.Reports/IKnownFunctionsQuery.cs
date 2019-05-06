using System.Collections.Generic;
using System.Threading.Tasks;
using MightyCalc.Calculations;

namespace MightyCalc.Reports
{
    public interface IKnownFunctionsQuery
    {
        Task<IReadOnlyCollection<FunctionDefinition>> Execute(string calculatorId=null, string functionName=null);
    }
}