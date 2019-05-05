using GridDomain.Aggregates;

namespace MightyCalc.Calculations.Aggregate.Commands
{
    public class AddFunction : Command<Calculator>
    {
        public FunctionDefinition Definition { get; }

        public AddFunction(string calculatorId, FunctionDefinition definition) : base(calculatorId)
        {
            Definition = definition;
        }
    }
}