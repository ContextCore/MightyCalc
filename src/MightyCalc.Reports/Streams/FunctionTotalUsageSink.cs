using System;
using Akka;
using Akka.Actor;
using Akka.Streams.Dsl;

namespace MightyCalc.Reports.Streams
{
    public static class FunctionTotalUsageSink
    {
        public static Sink<SequencedFunctionUsage, NotUsed> Create(IActorRefFactory system, string eventName)
        {
            var actorRef = system.ActorOf(Props.Create<FunctionsTotalUsageProjector>(eventName), nameof(FunctionsTotalUsageProjector));

            return Sink.ActorRefWithAck<SequencedFunctionUsage>(
                actorRef,
                FunctionsTotalUsageProjector.Start.Instance,
                FunctionsTotalUsageProjector.Next.Instance,
                FunctionsTotalUsageProjector.ProjectionDone.Instance);

        }
    }
}