using System;
using Akka.Actor;
using Akka.Cluster.Sharding;
using MightyCalc.Node;

namespace MightyCalc.API
{
    public class AkkaRemoteCalculatorPool : INamedCalculatorPool
    {
        private readonly IActorRef _region;
        private readonly TimeSpan? _timeout;

        public AkkaRemoteCalculatorPool(ActorSystem sys, TimeSpan? timeout = null)
        {
            _timeout = timeout;
            _region = ClusterSharding.Get(sys).StartProxy(typeof(CalculatorActor).Name,KnownRoles.Api, new ShardedMessageMetadataExtractor());
        }

        public IRemoteCalculator For(string name)
        {
            return new AkkaRemoteCalculator(name,_region,_timeout);
        }
    }
}