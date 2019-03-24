using System.Collections.Generic;

namespace MightyCalc.Calculations
{
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
}