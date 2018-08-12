using ProtoBuf;

namespace RedisClient
{
    [ProtoContract]
    public class MessageData
    {
        [ProtoMember(1)]
        public long Timestamp { get; set; }
        [ProtoMember(2)]
        public string Content { get; set; }
    }
}
