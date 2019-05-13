using Akka;
using Akka.Actor;
using Akka.Streams.Dsl;
using MightyCalc.Calculations.Aggregate.Events;
using MightyCalc.Node;
using MightyCalc.Node.Akka;
using MightyCalc.Reports.Streams.Projectors;

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
    
    public static class KnownFunctionsSink
    {
        public static Sink<FunctionAdded, NotUsed> Create(IActorRefFactory system, string eventName)
        {
            var actorRef = system.ActorOf(Props.Create<KnownFunctionsProjector>(eventName), nameof(KnownFunctionsProjector));

            return Sink.ActorRefWithAck<FunctionAdded>(
                actorRef,
                ProjectorActorProtocol.Start.Instance,
                ProjectorActorProtocol.Next.Instance,
                ProjectorActorProtocol.ProjectionDone.Instance);

        }
    }
}