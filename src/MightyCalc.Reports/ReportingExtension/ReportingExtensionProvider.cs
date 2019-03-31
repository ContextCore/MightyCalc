using Akka.Actor;
using Autofac;

namespace MightyCalc.Reports.ReportingExtension {
    public class ReportingExtensionProvider : ExtensionIdProvider<ReportingExtension>
    {
        private readonly IContainer _container;

        public ReportingExtensionProvider(IContainer container)
        {
            _container = container;
        }

        public override ReportingExtension CreateExtension(ExtendedActorSystem system)
        {
            return new ReportingExtension(system, _container);
        }
    }
}