using Akka.Actor;

namespace MightyCalc.Reports.ReportingExtension {
    public class ReportingExtensionProvider : ExtensionIdProvider<ReportingExtension>
    {
        private readonly IReportingDependencies _reportingDeps;

        public ReportingExtensionProvider(IReportingDependencies reportingDeps)
        {
            _reportingDeps = reportingDeps;
        }

        public override ReportingExtension CreateExtension(ExtendedActorSystem system)
        {
            return new ReportingExtension(system, _reportingDeps);
        }
    }
}