using System;
using GridDomain.Aggregates;

namespace MightyCalc.Calculations.Aggregate.Events
{
    public class CalculationPerformed : DomainEvent<Calculator>
    {
        public string Expression { get; }
        public double Value { get; }
        public Parameter[] Parameters { get; }
        public string[] FunctionsUsed { get; }

        public CalculationPerformed(string calculatorId, long version, string expression, Parameter[] parameters, double value,
            string[] functionsUsed) : base(calculatorId, version)
        {
            Expression = expression;
            Parameters = parameters;
            FunctionsUsed = functionsUsed;
            Value = value;
        }

        public DateTimeOffset Occured { get; } = DateTimeOffset.Now;
    }
}