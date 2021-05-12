using FluentAssertions;

using NUnit.Framework;

using static System.Text.Json.JsonSerializer;

namespace Brighid.Discord.Models
{
    public class EndpointSerializationTests
    {
        [Test, Auto]
        public void ChannelCreateShouldDeserialize()
        {
            var result = Deserialize<Endpoint>("\"c1\"");

            result.Category.Should().Be('c');
            result.Value.Should().Be(ChannelEndpoint.CreateMessage);
        }

        [Test, Auto]
        public void ChannelDeleteShouldDeserialize()
        {
            var result = Deserialize<Endpoint>("\"c2\"");

            result.Category.Should().Be('c');
            result.Value.Should().Be(ChannelEndpoint.DeleteMessage);
        }
    }
}
