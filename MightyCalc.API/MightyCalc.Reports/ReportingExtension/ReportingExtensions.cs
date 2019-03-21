using System;
using Akka.Actor;
using Autofac;

namespace MightyCalc.Reports.ReportingExtension {
    public static class ReportingExtensions
    {
        public static ReportingExtension GetReportingExtension(this ActorSystem sys)
        {
            return sys.GetExtension<ReportingExtension>();
        }

            
        public static ReportingExtension InitReportingExtension(this ActorSystem system,
                                                                  IContainer container)
        {
            if(system == null)
                throw new ArgumentNullException(nameof(system));

            return (ReportingExtension)system.RegisterExtension(new ReportingExtensionProvider(container));
        }
    }
}