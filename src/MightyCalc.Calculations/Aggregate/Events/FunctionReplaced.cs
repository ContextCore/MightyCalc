using GridDomain.Aggregates;

namespace MightyCalc.Calculations.Aggregate.Events
{
    public class FunctionReplaced : DomainEvent<Calculator>
    {
        public string CalculatorId { get; }
        public FunctionDefinition NewDefinition { get; }

        public FunctionReplaced(string calculatorId, FunctionDefinition definition, long version) : base(calculatorId, version)
        {
            CalculatorId = calculatorId;
            NewDefinition = definition;
        }
    }
}