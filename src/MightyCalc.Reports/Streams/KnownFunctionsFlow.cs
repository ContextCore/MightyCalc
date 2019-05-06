using Akka;
using Akka.Persistence.Query;
using Akka.Streams.Dsl;
using MightyCalc.Calculations.Aggregate.Events;

namespace MightyCalc.Reports.Streams
{
    public static class KnownFunctionsFlow
    {

        private static Flow<EventEnvelope, FunctionAdded, NotUsed> CreateFlow()
        {
            return Flow.Create<EventEnvelope>()
                .Select(e => e.Event as FunctionAdded);
        }

        public static Flow<EventEnvelope, FunctionAdded, NotUsed> Instance { get; } = CreateFlow();

    }
}