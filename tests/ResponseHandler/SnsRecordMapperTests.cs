using Amazon.Lambda.SNSEvents;

using AutoFixture.NUnit3;

using Brighid.Discord.Models;
using Brighid.Discord.Models.Payloads;
using Brighid.Discord.Serialization;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

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
                result.Endpoint.Category.Should().Be('c');
                result.Endpoint.Value.Should().Be(ChannelEndpoint.CreateMessage);
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
                string expectedBody,
                SNSEvent.SNSRecord record,
                [Frozen] ISerializer serializer,
                [Target] SnsRecordMapper mapper
            )
            {
                serializer.Serialize(Any<CreateMessagePayload>()).Returns(expectedBody);

                record.Sns.Message = expectedBody;
                record.Sns.MessageAttributes["Brighid.SourceId"] = new SNSEvent.MessageAttribute { Type = "String", Value = channelId };

                var result = mapper.MapToRequest(record);
                result.RequestBody.Should().Be(expectedBody);
            }
        }
    }
}
