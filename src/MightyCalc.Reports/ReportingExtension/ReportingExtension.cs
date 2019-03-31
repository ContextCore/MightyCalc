using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using Akka.Persistence;
using Autofac;

namespace MightyCalc.Reports.ReportingExtension
{
    public class ReportingExtension : IExtension
    {
        private readonly IContainer _container;
        private readonly ExtendedActorSystem _system;
        public IActorRef ReportingActor { get; private set; }

        public ReportingExtension(ExtendedActorSystem system, IContainer container)
        {
            _system = system;
            _container = container;
        }

        public IActorRef Start()
        {
            
            var dispatcher = _system.ActorOf(ClusterSingletonManager.Props(
                    Props.Create<ReportingActor>(),
                    PoisonPill.Instance,
                    ClusterSingletonManagerSettings.Create(_system)
                ),
                name: "reporting");
      
            ReportingActor = _system.ActorOf(ClusterSingletonProxy.Props(
                    singletonManagerPath: "/user/reporting",
                    settings: ClusterSingletonProxySettings.Create(_system)),
                name: "reportingProxy");

            ReportingActor.Tell(Reports.ReportingActor.Start.Instance);
            return ReportingActor;
        }
        
        public IReportingDependencies GetDependencies()
        {
            return _container.Resolve<IReportingDependencies>();
        }
    }
}