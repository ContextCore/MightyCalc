using System.Threading.Tasks;
using GridDomain.Aggregates;
using MightyCalc.Calculations.Aggregate.Commands;

namespace MightyCalc.Node.Domain
{
    public class CalculatorCommandsHandler:ICommandHandler<ICommand>
    {
        private readonly ICommandHandler<ICommand> _handler;

        public CalculatorCommandsHandler(ICommandHandler<ICommand> handler)
        {
            _handler = handler;
        }
        public async Task<object> Execute(ICommand command)
        {
            switch (command)
            {
                case CalculateExpression c:
                    return await Execute(c);
                default:
                    return await _handler.Execute(command);
            }
        }

        public async Task<double> Execute(CalculateExpression command)
        {
            return (double) await _handler.Execute(command);
        }
    }
}