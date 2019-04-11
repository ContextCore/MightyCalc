using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using Akka.Persistence;

namespace MightyCalc.Reports.ReportingExtension
{
    public class ReportingExtension : IExtension
    {
        private readonly IReportingDependencies _dependencies;
        private readonly ExtendedActorSystem _system;
        public IActorRef ReportingActor { get; private set; }

        public ReportingExtension(ExtendedActorSystem system, IReportingDependencies dependencies)
        {
            _system = system;
            _dependencies = dependencies;
        }

        public IActorRef Start()
        {
            
            var dispatcher = _system.ActorOf(ClusterSingletonManager.Props(
                    Props.Create<ReportingActor>(),
                    PoisonPill.Instance,
                    ClusterSingletonManagerSettings.Create(_system).WithRole(KnownRoles.Projection)
                ),
                name: "reporting");
      
            ReportingActor = _system.ActorOf(ClusterSingletonProxy.Props(
                    singletonManagerPath: "/user/reporting",
                    settings: ClusterSingletonProxySettings.Create(_system).WithRole(KnownRoles.Projection)),
                name: "reportingProxy");

            ReportingActor.Tell(Reports.ReportingActor.Start.Instance);
            return ReportingActor;
        }
        
        public IReportingDependencies GetDependencies()
        {
            return _dependencies;
        }
    }
}