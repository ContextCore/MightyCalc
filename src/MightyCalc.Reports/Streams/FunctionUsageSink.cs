using Akka;
using Akka.Actor;
using Akka.Streams.Dsl;
using MightyCalc.Node;

namespace MightyCalc.Reports.Streams
{
    public static class FunctionUsageSink
    {
        public static Sink<Sequenced<CalculatorActor.CalculationPerformed>, NotUsed> Create(IActorRefFactory system, string eventName)
        {
            var actorRef = system.ActorOf(Props.Create<FunctionsUsageProjector>(eventName, null), nameof(FunctionsUsageProjector));

            return Sink.ActorRefWithAck<Sequenced<CalculatorActor.CalculationPerformed>>(
                actorRef,
                ProjectorActorProtocol.Start.Instance,
                ProjectorActorProtocol.Next.Instance,
                ProjectorActorProtocol.ProjectionDone.Instance);

        }
    }
}