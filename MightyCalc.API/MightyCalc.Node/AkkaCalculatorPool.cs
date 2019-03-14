using Akka.Actor;
using Akka.Cluster.Sharding;

namespace MightyCalc.Node
{
    public class AkkaCalculatorPool : INamedCalculatorPool
    {
        private readonly IActorRef _region;

        public AkkaCalculatorPool(ActorSystem sys)
        {
            var clusterSharding = ClusterSharding.Get(sys);


            var aggregateProps = Props.Create<CalculatorActor>();

            _region = clusterSharding.StartAsync("calculators",
                aggregateProps,
                ClusterShardingSettings.Create(sys),
                new ShardedMessageMetadataExtractor()).Result; 
        }

        public IRemoteCalculator For(string name)
        {
            return new AkkaRemoteCalculator(name,_region);
        }
    }
}