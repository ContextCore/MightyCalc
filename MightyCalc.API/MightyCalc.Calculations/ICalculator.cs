using System.Collections.Generic;

namespace MightyCalc.Calculations
{
    public interface ICalculator
    {
        CalculationResult Calculate(string expression, params Parameter[] parameters);
        void AddFunction(string name, string description, string expression, params string[] parameterNames);

        IReadOnlyList<FunctionDefinition> GetKnownFunctions();
    }

    public class CalculationResult
    {
        public CalculationResult(double value, IReadOnlyCollection<string> functionUsages)
        {
            Value = value;
            FunctionUsages = functionUsages;
        }

        public IReadOnlyCollection<string> FunctionUsages { get; }
        public double Value { get; }
    }


   
    public class FunctionDefinition
    {
        public FunctionDefinition(string name, int arity, string description, string expression, params string [] parameters)
        {
            Name = name;
            Arity = arity;
            Description = description;
            Expression = expression;
            Parameters = parameters;
        }

        public string Name { get; }
        public string Description { get; }
        public int Arity { get; }
        public string Expression { get; }
        public string[] Parameters { get; }
    }
}