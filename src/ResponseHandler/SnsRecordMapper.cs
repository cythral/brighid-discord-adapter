using Amazon.Lambda.SNSEvents;

using Brighid.Discord.Models;
using Brighid.Discord.Models.Payloads;
using Brighid.Discord.Serialization;

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

        public Request MapToRequest(SNSEvent.SNSRecord record)
        {
            return new Request(ChannelEndpoint.CreateMessage)
            {
                Parameters = { ["channel.id"] = record.Sns.MessageAttributes["Brighid.SourceId"].Value },
                RequestBody = serializer.Serialize(new CreateMessagePayload
                {
                    Content = record.Sns.Message,
                }),
            };
        }
    }
}
