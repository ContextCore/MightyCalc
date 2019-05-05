using System.Collections.Generic;
using System.Linq;
using GridDomain.Aggregates;
using MightyCalc.Calculations.Aggregate.Commands;
using MightyCalc.Calculations.Aggregate.Events;

namespace MightyCalc.Node.Domain
{
    public class CalculatorCommandResultAdapter : ICommandsResultAdapter
    {
        public object Adapt(object command, IReadOnlyCollection<GridDomain.Aggregates.IDomainEvent> result)
        {
            switch (command)
            {
                case CalculateExpression c:
                    return result.OfType<CalculationPerformed>().FirstOrDefault()?.Value;
                default:
                    return CommandsResultNullAdapter.Instance.Adapt(command, result);
            }
        }
    }
}