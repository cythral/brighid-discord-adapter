using System.Text.Json;

using Amazon.Lambda.SNSEvents;

using Brighid.Discord.Models;
using Brighid.Discord.Models.Payloads;

namespace Brighid.Discord.Adapter.ResponseHandler
{
    /// <inheritdoc />
    public class SnsRecordMapper : ISnsRecordMapper
    {
        public Request MapToRequest(SNSEvent.SNSRecord record)
        {
            return new Request(ChannelEndpoint.CreateMessage)
            {
                Parameters = { ["channel.id"] = record.Sns.MessageAttributes["Brighid.SourceId"].Value },
                RequestBody = JsonSerializer.Serialize(new CreateMessagePayload
                {
                    Content = record.Sns.Message,
                }),
            };
        }
    }
}
