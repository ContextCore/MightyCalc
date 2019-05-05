using GridDomain.Aggregates;

namespace MightyCalc.Calculations.Aggregate.Commands
{
    public class CreateCalculator : Command<Calculator>
    {
        public CreateCalculator(string id):base(id)
        {
            
        }
    }
}