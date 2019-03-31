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
                var eventName = nameof(CalculatorActor.CalculationPerformed);

                Offset offset;
                using (var context = _dependencies.CreateFunctionUsageContext())
                {
                    var projection = _dependencies.CreateFindProjectionQuery(context).Execute(KnownProjectionsNames.TotalFunctionUsage,
                        nameof(FunctionsTotalUsageProjector),
                        eventName);

                    if (projection == null)
                    {
                        offset = Offset.NoOffset();
                        _log.Info(
                            $"Starting fresh projection '{KnownProjectionsNames.TotalFunctionUsage}' from start");
                    }
                    else
                    {
                        offset = Offset.Sequence(projection.Sequence+1);
                        _log.Info(
                            $"Starting projection '{KnownProjectionsNames.TotalFunctionUsage}' from sequence {projection.Sequence+1}");
                    }
                }

                var source = _readJournal.EventsByTag(eventName, offset);
                var groupingFlow = FunctionTotalUsageFlow.Instance;
                var projectionSink = FunctionTotalUsageSink.Create(Context, eventName);
                
                var projectionGraph = source.Via(groupingFlow).To(projectionSink);
                _materializer = Context.System.Materializer();
                projectionGraph.Run(_materializer);
                Behavior.Become(Working, nameof(Working));
            });
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