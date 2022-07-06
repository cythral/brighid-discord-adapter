using AutoFixture.NUnit3;

using Brighid.Discord.Models;
using Brighid.Discord.Models.Payloads;
using Brighid.Discord.Serialization;

using FluentAssertions;

using Lambdajection.Sns;

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
                SnsMessage<string> record,
                [Target] SnsRecordMapper mapper
            )
            {
                record.MessageAttributes["Brighid.SourceId"] = new SnsMessageAttribute("String", channelId);

                var result = mapper.MapToRequest(record);
                result.Endpoint.Category.Should().Be('c');
                result.Endpoint.Value.Should().Be(ChannelEndpoint.CreateMessage);
            }

            [Test, Auto]
            public void ShouldSetChannelIdParameter(
                string channelId,
                SnsMessage<string> record,
                [Target] SnsRecordMapper mapper
            )
            {
                record.MessageAttributes["Brighid.SourceId"] = new SnsMessageAttribute("String", channelId);

                var result = mapper.MapToRequest(record);
                result.Parameters["channel.id"].Should().Be(channelId);
            }

            [Test, Auto]
            public void ShouldSetBody(
                string channelId,
                string expectedBody,
                SnsMessage<string> record,
                [Frozen] ISerializer serializer,
                [Target] SnsRecordMapper mapper
            )
            {
                serializer.Serialize(Any<CreateMessagePayload>()).Returns(expectedBody);

                record.Message = expectedBody;
                record.MessageAttributes["Brighid.SourceId"] = new SnsMessageAttribute("String", channelId);

                var result = mapper.MapToRequest(record);
                result.RequestBody.Should().Be(expectedBody);
            }
        }
    }
}
