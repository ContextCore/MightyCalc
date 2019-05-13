using System.Threading.Tasks;
using GridDomain.Aggregates;
using GridDomain.Domains;
using MightyCalc.Calculations.Aggregate;

namespace MightyCalc.Node.Domain
{
    public class CalculatorDomainConfiguration : IDomainConfiguration
    {
        public async Task Register(IDomainBuilder builder)
        {
            await builder.RegisterAggregate(new AggregateConfiguration<Calculator>(null, new AggregateSettings(hostRole:"calculation")));
            builder.RegisterCommandsResultAdapter<Calculator>(new CalculatorCommandResultAdapter());
            builder.RegisterCommandHandler(handler => new CalculatorCommandsHandler(handler));
        }
    }
}