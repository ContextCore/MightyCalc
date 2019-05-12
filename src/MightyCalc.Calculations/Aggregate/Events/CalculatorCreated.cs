using GridDomain.Aggregates;

namespace MightyCalc.Calculations.Aggregate.Events
{
    public class CalculatorCreated : DomainEvent<Calculator>
    {
        public CalculatorCreated(string source, long version) : base(source, version)
        {
        }
    }
}