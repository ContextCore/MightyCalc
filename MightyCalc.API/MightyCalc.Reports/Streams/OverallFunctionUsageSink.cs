using Akka;
using Akka.Actor;
using Akka.Streams.Dsl;

namespace MightyCalc.Reports.Streams
{
    public static class OverallFunctionUsageSink
    {
        public static Sink<SequencedUsage, NotUsed> Create(IActorRefFactory system, string eventName)
        {
            return Sink.ActorRefWithAck<SequencedUsage>(
                system.ActorOf(Props.Create<FunctionsTotalUsageProjector>(eventName), "overallUsageProjector"),
                FunctionsTotalUsageProjector.Start.Instance,
                FunctionsTotalUsageProjector.Next.Instance,
                FunctionsTotalUsageProjector.ProjectionDone.Instance);
        }
    }
}