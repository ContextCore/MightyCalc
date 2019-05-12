using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Sharding;
using GridDomain.Domains;
using GridDomain.Node;
using GridDomain.Node.Akka.GridDomainNodeExtension;

namespace MightyCalc.Node.Domain
{
    public class DomainCalculatorPool : INamedCalculatorPool
    {
        private IDomain _domain;

        public DomainCalculatorPool(IDomain domain)
        {
            _domain = domain;
        }

        public IRemoteCalculator For(string name)
        {
            return new DomainCalculator(name, _domain.CommandHandler<CalculatorCommandsHandler>());
        }
    }
}