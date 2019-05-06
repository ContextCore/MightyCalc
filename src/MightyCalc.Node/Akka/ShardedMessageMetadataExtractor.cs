using Akka.Cluster.Sharding;

namespace MightyCalc.Node.Akka
{
    public class ShardedMessageMetadataExtractor : IMessageExtractor
    {
        public string EntityId(object message) => (message as ShardEnvelop)?.EntityId;

        public object EntityMessage(object message) => (message as ShardEnvelop)?.Message;

        public string ShardId(object message)=> (message as ShardEnvelop)?.ShardId;
    }
}