using System.Threading.Tasks;
using Akka.Actor;
using GridDomain.Domains;
using GridDomain.Node;
using GridDomain.Node.Akka.GridDomainNodeExtension;

namespace MightyCalc.Node.Domain
{
    public class DomainCalculatorPool : INamedCalculatorPool
    {
        private readonly GridDomainNode _node;
        private IDomain domain;

        public DomainCalculatorPool(ActorSystem sys)
        {
            _node = sys.InitGridDomainExtension(new CalculatorDomainConfiguration());
        }

        public async Task Start()
        {
            domain = await _node.Start();
        }

        public IRemoteCalculator For(string name)
        {
            return new DomainCalculator(name, domain.CommandHandler<CalculatorCommandsHandler>());
        }
    }
}