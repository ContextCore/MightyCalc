using System.Threading.Tasks;
using MightyCalc.Calculations;

namespace MightyCalc.Node
{
    public interface IRemoteCalculator
    {
        Task<double> Calculate(string expression, params Parameter[] parameters);

        Task AddFunction(string functionName, string description, string expression,
            params string[] parameterNames);
        
        Task<FunctionDefinition[]> GetKnownFunction(string functionName);
    }
}