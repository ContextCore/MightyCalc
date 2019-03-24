namespace MightyCalc.Node
{
    class ShardEnvelop
    {
        public ShardEnvelop(string entityId, string shardId, object message)
        {
            EntityId = entityId;
            ShardId = shardId;
            Message = message;
        }

        public string EntityId { get; }
        public string ShardId { get; }
        public object Message { get; }
    }
}