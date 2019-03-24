using Akka.Actor;
using Akka.Persistence;
using Autofac;

namespace MightyCalc.Reports.ReportingExtension
{
    public class ReportingExtension : IExtension
    {
        private readonly IContainer _container;

        public ReportingExtension(IContainer container)
        {
            _container = container;
        }

        public IReportingDependencies GetDependencies()
        {
            return _container.Resolve<IReportingDependencies>();
        }
    }
}