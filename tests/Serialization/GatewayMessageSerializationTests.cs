using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Events;
using Brighid.Discord.Messages;
using Brighid.Discord.Models;

using FluentAssertions;

using NUnit.Framework;

namespace Brighid.Discord.Serialization
{
    [Category("Integration")]
    public class GatewayMessageSerializationTests
    {
        [Test, Auto]
        public async Task HelloMessagesShouldDeserialize(
            uint interval,
            int sequenceNumber
        )
        {
            var cancellationToken = new CancellationToken(false);
            var serializer = CreateSerializer();
            var payload = CreatePayloadStream($@"{{""op"": 10, ""s"": {sequenceNumber}, ""d"": {{ ""heartbeat_interval"": {interval} }} }}");
            var result = await serializer.Deserialize<GatewayMessage>(payload, cancellationToken);

            result.OpCode.Should().Be(GatewayOpCode.Hello);
            result.SequenceNumber.Should().Be(sequenceNumber);
            result.Data.Should().BeOfType<HelloEvent>();

            var helloEvent = result.Data.As<HelloEvent>();
            helloEvent.HeartbeatInterval.Should().Be(interval);
        }

        [Test, Auto]
        public async Task HelloMessagesShouldSerialize(
            uint interval,
            int sequenceNumber
        )
        {
            var cancellationToken = new CancellationToken(false);
            var serializer = CreateSerializer();
            var gatewayMessage = new GatewayMessage
            {
                OpCode = GatewayOpCode.Hello,
                SequenceNumber = sequenceNumber,
                Data = new HelloEvent
                {
                    HeartbeatInterval = interval,
                },
            };

            var result = await serializer.Serialize(gatewayMessage, cancellationToken);
            result.Should().MatchRegex("\"op\":[ ]?10", "Op Code should equal 10.");
            result.Should().MatchRegex($"\"s\":[ ]?{sequenceNumber}", $"Sequence number should equal {sequenceNumber}");
            result.Should().MatchRegex($"\"d\":[ ]?{{[ ]?\"heartbeat_interval\":[ ]?{interval}[ ]?}}", $"Data should contain heartbeat interval of {interval}");
        }

        [Test, Auto]
        public async Task ReadyMessagesShouldDeserialize(
            uint version,
            ulong userId,
            int sequenceNumber
        )
        {
            var cancellationToken = new CancellationToken(false);
            var serializer = CreateSerializer();
            var payload = CreatePayloadStream($@"{{
                ""op"": 0, 
                ""t"": ""READY"", 
                ""s"": {sequenceNumber}, 
                ""d"": {{ 
                    ""v"": {version},
                    ""user"": {{
                        ""id"": ""{userId}""
                    }}
                }}
            }}");
            var result = await serializer.Deserialize<GatewayMessage>(payload, cancellationToken);

            result.OpCode.Should().Be(GatewayOpCode.Dispatch);
            result.SequenceNumber.Should().Be(sequenceNumber);
            result.Data.Should().BeOfType<ReadyEvent>();

            var readyEvent = result.Data.As<ReadyEvent>();
            readyEvent.GatewayVersion.Should().Be(version);
            readyEvent.User.Id.Should().Be(new Snowflake(userId));
        }

        [Test, Auto]
        public async Task ReadyMessagesShouldSerialize(
            uint version,
            ulong userId,
            int sequenceNumber
        )
        {
            var cancellationToken = new CancellationToken(false);
            var serializer = CreateSerializer();
            var gatewayMessage = new GatewayMessage
            {
                OpCode = GatewayOpCode.Dispatch,
                SequenceNumber = sequenceNumber,
                EventName = "READY",
                Data = new ReadyEvent
                {
                    GatewayVersion = version,
                    User = new User
                    {
                        Id = userId,
                    },
                },
            };

            var result = await serializer.Serialize(gatewayMessage, cancellationToken);
            result.Should().MatchRegex("\"op\":[ ]?0", "Op Code should equal 0.");
            result.Should().MatchRegex($"\"s\":[ ]?{sequenceNumber}", $"Sequence number should equal {sequenceNumber}");
            result.Should().MatchRegex($"\"t\":[ ]?\"READY\"", $"Event name should equal READY");
            result.Should().MatchRegex($"\"d\":[ ]?{{(.*?)\"v\":[ ]?{version}(.*?)}}", $"Data should contain version of {version}");
            result.Should().MatchRegex($"\"d\":[ ]?{{(.*?)\"user\":[ ]?{{(.*?)\"id\":[ ]?\"{userId}\"(.*?)}}(.*?)}}", $"Data should contain userId of {userId}");
        }

        [Test, Auto]
        public async Task HeartbeatMessagesShouldSerialize(
            int sequenceNumber
        )
        {
            var cancellationToken = new CancellationToken(false);
            var serializer = CreateSerializer();
            var gatewayMessage = new GatewayMessage
            {
                OpCode = GatewayOpCode.Heartbeat,
                Data = new HeartbeatEvent(sequenceNumber),
            };

            var result = await serializer.Serialize(gatewayMessage, cancellationToken);
            result.Should().MatchRegex("\"op\":[ ]?1", "Op Code should equal 1.");
            result.Should().MatchRegex($"\"d\":[ ]?{sequenceNumber}", $"Data should equal the sequence number ({sequenceNumber})");
        }

        private Stream CreatePayloadStream(string payload)
        {
            var bytes = Encoding.UTF8.GetBytes(payload);
            return new MemoryStream(bytes);
        }

        private ISerializer CreateSerializer()
        {
            var messageParser = new GeneratedMessageParser();
            var messageConverter = new GatewayMessageConverter(messageParser);
            return new JsonSerializer(messageConverter);
        }
    }
}
