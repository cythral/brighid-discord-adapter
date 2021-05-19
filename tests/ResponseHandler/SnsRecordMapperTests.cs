using System.Text.Json;

using Amazon.Lambda.SNSEvents;

using Brighid.Discord.Models;
using Brighid.Discord.Models.Payloads;

using FluentAssertions;

using NUnit.Framework;

namespace Brighid.Discord.Adapter.ResponseHandler
{
    public class SnsRecordMapperTests
    {
        [TestFixture]
        public class MapToRequestTests
        {
            [Test, Auto]
            public void ShouldSetEndpointToChannelCreateMessage(
                string channelId,
                SNSEvent.SNSRecord record,
                [Target] SnsRecordMapper mapper
            )
            {
                record.Sns.MessageAttributes["Brighid.SourceId"] = new SNSEvent.MessageAttribute { Type = "String", Value = channelId };

                var result = mapper.MapToRequest(record);
                result.Endpoint.Should().Be((Endpoint)ChannelEndpoint.CreateMessage);
            }

            [Test, Auto]
            public void ShouldSetChannelIdParameter(
                string channelId,
                SNSEvent.SNSRecord record,
                [Target] SnsRecordMapper mapper
            )
            {
                record.Sns.MessageAttributes["Brighid.SourceId"] = new SNSEvent.MessageAttribute { Type = "String", Value = channelId };

                var result = mapper.MapToRequest(record);
                result.Parameters["channel.id"].Should().Be(channelId);
            }

            [Test, Auto]
            public void ShouldSetBody(
                string channelId,
                string content,
                SNSEvent.SNSRecord record,
                [Target] SnsRecordMapper mapper
            )
            {
                record.Sns.Message = content;
                record.Sns.MessageAttributes["Brighid.SourceId"] = new SNSEvent.MessageAttribute { Type = "String", Value = channelId };

                var result = mapper.MapToRequest(record);
                var deserializedBody = JsonSerializer.Deserialize<CreateMessagePayload>(result.RequestBody!);
                deserializedBody.Content.Should().Be(content);
            }
        }
    }
}
