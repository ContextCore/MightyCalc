using Akka.Actor;
using Akka.Event;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Streams;
using Akka.Streams.Implementation.Fusing;
using MightyCalc.Node;
using MightyCalc.Reports.ReportingExtension;
using MightyCalc.Reports.Streams;

namespace MightyCalc.Reports
{
    public class ReportingActor : ReceiveActor
    {
        private readonly SqlReadJournal _readJournal;
        private ActorMaterializer _materializer;
        private readonly IReportingDependencies _dependencies;
        private readonly ILoggingAdapter _log;

        private BehaviorQueue Behavior { get; }

        public ReportingActor()
        {
            Behavior = new BehaviorQueue(Become);
            // obtain read journal by plugin id
            _readJournal = PersistenceQuery.Get(Context.System)
                .ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
            Behavior.Become(Initializing, nameof(Initializing));
            _dependencies = Context.System.GetReportingExtension().GetDependencies();
            _log = Context.GetLogger();
        }

        public void Initializing()
        {
            Receive<Start>(s =>
            {

                StartTotalUsageProjection();

                StartFunctionUsageProjection();

                Behavior.Become(Working, nameof(Working));
            });
        }

        private void StartFunctionUsageProjection()
        {
            var eventName = nameof(CalculatorActor.CalculationPerformed);
            var offset = GetProjectionOffset(eventName, KnownProjectionsNames.FunctionUsage, nameof(FunctionsUsageProjector));


            var source = _readJournal.EventsByTag(eventName, offset);
            var flow = FunctionUsageFlow.Instance;
            var sink = FunctionUsageSink.Create(Context, eventName);

            var projectionGraph = source.Via(flow).To(sink);
            _materializer = Context.System.Materializer();
            projectionGraph.Run(_materializer);
        }

        private Offset GetProjectionOffset(string eventName, string projectionName, string projectorName)
        {
            Offset offset;
            using (var context = _dependencies.CreateFunctionUsageContext())
            {
                var projection = _dependencies.CreateFindProjectionQuery(context).Execute(
                    projectionName,
                    projectorName,
                    eventName);

                if (projection == null)
                {
                    offset = Offset.NoOffset();
                    _log.Info(
                        $"Starting fresh projection '{projectionName}' from start");
                }
                else
                {
                    offset = Offset.Sequence(projection.Sequence + 1);
                    _log.Info(
                        $"Starting projection '{projectionName}' from sequence {projection.Sequence + 1}");
                }
            }

            return offset;
        }

        private void StartTotalUsageProjection()
        {
            var eventName = nameof(CalculatorActor.CalculationPerformed);

            var offset = GetProjectionOffset(eventName, KnownProjectionsNames.TotalFunctionUsage, nameof(FunctionsTotalUsageProjector));

            var source = _readJournal.EventsByTag(eventName, offset);
            var groupingFlow = FunctionTotalUsageFlow.Instance;
            var projectionSink = FunctionTotalUsageSink.Create(Context, eventName);
            var projectionGraph = source.Via(groupingFlow).To(projectionSink);
            _materializer = Context.System.Materializer();
            projectionGraph.Run(_materializer);
        }

        public void Working()
        {
        }

        protected override void PostStop()
        {
            _materializer?.Dispose();
            base.PostStop();
        }

        public class Start
        {
            private Start()
            {
            }

            public static Start Instance { get; } = new Start();
        }
    }
}