using System;
using Akka.Actor;

namespace MightyCalc.Reports.ReportingExtension {
    public static class ReportingExtensions
    {
        public static ReportingExtension GetReportingExtension(this ActorSystem sys)
        {
            return sys.GetExtension<ReportingExtension>();
        }

            
        public static ReportingExtension InitReportingExtension(this ActorSystem system,
                                                                IReportingDependencies container)
        {
            if(system == null)
                throw new ArgumentNullException(nameof(system));

            return (ReportingExtension)system.RegisterExtension(new ReportingExtensionProvider(container));
        }
    }
}