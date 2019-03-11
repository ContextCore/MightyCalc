using System.Collections.Generic;

namespace MightyCalc.Calculations
{
    public interface ICalculator
    {
        double Calculate(string expression, params Parameter[] parameters);
        void AddFunction(string name, string description, string expression, params string[] parameterNames);

        IReadOnlyList<FunctionSignature> GetKnownFunctions();
    }

    public class FunctionSignature
    {
        public FunctionSignature(string name, int arity, string description)
        {
            Name = name;
            Arity = arity;
            Description = description;
        }

        public string Name { get; }
        public string Description { get; }
        public int Arity { get; }
    }
}