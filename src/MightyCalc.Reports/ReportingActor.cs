using Akka;
using Akka.Actor;
using Akka.Event;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.Implementation.Fusing;
using MightyCalc.Calculations.Aggregate.Events;
using MightyCalc.Node;
using MightyCalc.Node.Akka;
using MightyCalc.Reports.ReportingExtension;
using MightyCalc.Reports.Streams;
using MightyCalc.Reports.Streams.Projectors;

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
            _readJournal = PersistenceQuery.Get(Context.System).ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
            Behavior.Become(Initializing, nameof(Initializing));
            _dependencies = Context.System.GetReportingExtension().GetDependencies();
            _log = Context.GetLogger();
        }

        public void Initializing()
        {
            Receive<Start>(s =>
            {
                _log.Info("Starting projections");
                
                StartTotalUsageProjection();

                StartFunctionUsageProjection();
                
                StartKnownFunctionsProjection();

                Behavior.Become(Working, nameof(Working));
            });
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

        private void StartFunctionUsageProjection()
        {
            var eventName = nameof(CalculatorActor.CalculationPerformed);
            StartProjection(eventName,
                KnownProjectionsNames.FunctionUsage,
                nameof(FunctionsUsageProjector),
                FunctionUsageFlow.Instance,
                FunctionUsageSink.Create(Context, eventName));
        }
        
        private void StartTotalUsageProjection()
        {
            var eventName = nameof(CalculatorActor.CalculationPerformed);
            StartProjection(eventName,
                KnownProjectionsNames.TotalFunctionUsage,
                nameof(FunctionsTotalUsageProjector),
                FunctionTotalUsageFlow.Instance,
                FunctionTotalUsageSink.Create(Context, eventName));

        }
        
        private void StartProjection<TEvent>(string eventName, 
                                             string projectionName, 
                                             string projectorName,
                                             Flow<EventEnvelope,TEvent,NotUsed> flow,
                                             Sink<TEvent,NotUsed> sink
                                             )
        {
            var offset = GetProjectionOffset(eventName, projectionName, projectorName);
            var source = _readJournal.EventsByTag(eventName, offset);
            var projectionGraph = source.Via(flow).To(sink);
            _materializer = Context.System.Materializer();
            projectionGraph.Run(_materializer);
        }
        
        private void StartKnownFunctionsProjection()
        {
            var eventName = nameof(FunctionAdded);
            StartProjection(eventName,
                KnownProjectionsNames.KnownFunctions,
                nameof(KnownFunctionsProjector),
                KnownFunctionsFlow.Instance,
                KnownFunctionsSink.Create(Context, eventName));
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