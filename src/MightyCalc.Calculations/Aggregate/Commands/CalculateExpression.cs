using GridDomain.Aggregates;

namespace MightyCalc.Calculations.Aggregate.Commands
{
    public class CalculateExpression : Command<Calculator>
    {
        public string Representation { get; }
        public Parameter[] Parameters { get; }

        public CalculateExpression(string calculatorId, string representation, params Parameter[] parameters) : base(
            calculatorId)
        {
            Representation = representation;
            Parameters = parameters;
        }
    }
}