using System;
using Akka.Actor;
using Akka.Cluster.Sharding;

namespace MightyCalc.Node.Akka
{
    public class AkkaCalculatorPool : INamedCalculatorPool
    {
        private readonly IActorRef _region;
        private readonly TimeSpan? _timeout;

        public AkkaCalculatorPool(ActorSystem sys, TimeSpan? timeout = null)
        {
            _timeout = timeout;
            var clusterSharding = ClusterSharding.Get(sys);


            var entityProps = Props.Create<CalculatorActor>();

            _region = clusterSharding.StartAsync(KnownActorRegions.Calculator,
                entityProps,
                ClusterShardingSettings.Create(sys).WithRole(KnownRoles.Calculation),
                new ShardedMessageMetadataExtractor()).Result; 
        }

        public IRemoteCalculator For(string name)
        {
            return new AkkaRemoteCalculator(name,_region,_timeout);
        }
    }
}