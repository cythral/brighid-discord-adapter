using Brighid.Discord.Models;
using Brighid.Discord.Models.Payloads;
using Brighid.Discord.Serialization;

using Lambdajection.Sns;

namespace Brighid.Discord.Adapter.ResponseHandler
{
    /// <inheritdoc />
    public class SnsRecordMapper : ISnsRecordMapper
    {
        private readonly ISerializer serializer;

        public SnsRecordMapper(
            ISerializer serializer
        )
        {
            this.serializer = serializer;
        }

        public Request MapToRequest(SnsMessage<string> record)
        {
            return new Request(ChannelEndpoint.CreateMessage)
            {
                Parameters = { ["channel.id"] = record.MessageAttributes["Brighid.SourceId"].Value },
                RequestBody = serializer.Serialize(new CreateMessagePayload
                {
                    Content = record.Message,
                }),
            };
        }
    }
}
