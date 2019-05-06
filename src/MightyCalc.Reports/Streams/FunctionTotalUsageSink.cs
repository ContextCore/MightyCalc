using System;
using Akka;
using Akka.Actor;
using Akka.Streams.Dsl;
using MightyCalc.Reports.Streams.Projectors;

namespace MightyCalc.Reports.Streams
{
    public static class FunctionTotalUsageSink
    {
        public static Sink<SequencedFunctionTotalUsage, NotUsed> Create(IActorRefFactory system, string eventName)
        {
            var actorRef = system.ActorOf(Props.Create<FunctionsTotalUsageProjector>(eventName), nameof(FunctionsTotalUsageProjector));

            return Sink.ActorRefWithAck<SequencedFunctionTotalUsage>(
                actorRef,
                ProjectorActorProtocol.Start.Instance,
                ProjectorActorProtocol.Next.Instance,
                ProjectorActorProtocol.ProjectionDone.Instance);

        }
    }
}