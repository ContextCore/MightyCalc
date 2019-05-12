using GridDomain.Aggregates;

namespace MightyCalc.Calculations.Aggregate.Events
{
    public class FunctionAdded : DomainEvent<Calculator>
    {
        public string CalculatorId { get; }
        public FunctionDefinition Definition { get; }

        public FunctionAdded(string calculatorId, FunctionDefinition definition, long version) : base(calculatorId, version)
        {
            CalculatorId = calculatorId;
            Definition = definition;
        }
    }
}