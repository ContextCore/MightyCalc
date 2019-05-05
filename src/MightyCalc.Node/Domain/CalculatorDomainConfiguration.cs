using System.Threading.Tasks;
using GridDomain.Aggregates;
using GridDomain.Domains;
using MightyCalc.Calculations.Aggregate;

namespace MightyCalc.Node.Domain
{
    public class CalculatorDomainConfiguration : IDomainConfiguration
    {
        public Task Register(IDomainBuilder builder)
        {
            builder.RegisterAggregate(new AggregateConfiguration<Calculator>());
            builder.RegisterCommandsResultAdapter<Calculator>(new CalculatorCommandResultAdapter());
            builder.RegisterCommandHandler(handler => new CalculatorCommandsHandler(handler));
            return Task.CompletedTask;
        }
    }
}