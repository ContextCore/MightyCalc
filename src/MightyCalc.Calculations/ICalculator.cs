using System.Collections.Generic;

namespace MightyCalc.Calculations
{
    public interface ICalculator
    {
        CalculationResult Calculate(string expression, params Parameter[] parameters);
        void AddFunction(string name, string description, string expression, params string[] parameterNames);

        IReadOnlyList<FunctionDefinition> GetKnownFunctions();
    }
    
}