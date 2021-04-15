using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Events;
using Brighid.Discord.Serialization;

using FluentAssertions;

using NUnit.Framework;

namespace Brighid.Discord.Messages
{
    [Category("Integration")]
    public class GeneratedMessageParserTests
    {
        [Test, Auto]
        public async Task ParseShouldParseHelloMessages(
            uint interval,
            int sequenceNumber
        )
        {
            var cancellationToken = new CancellationToken(false);
            var parser = CreateParser();
            var payload = CreatePayloadStream($@"{{""op"": 10, ""s"": {sequenceNumber}, ""d"": {{ ""heartbeat_interval"": {interval} }} }}");
            var result = await parser.Parse(payload, cancellationToken);

            result.OpCode.Should().Be(GatewayOpCode.Hello);
            result.SequenceNumber.Should().Be(sequenceNumber);
            result.Data.Should().BeOfType<HelloEvent>();

            var helloEvent = result.Data.As<HelloEvent>();
            helloEvent.HeartbeatInterval.Should().Be(interval);
        }

        [Test, Auto]
        public async Task ParseShouldParseReadyMessages(
            uint version,
            ulong userId,
            int sequenceNumber
        )
        {
            var cancellationToken = new CancellationToken(false);
            var parser = CreateParser();
            var payload = CreatePayloadStream($@"{{
                ""op"": 0, 
                ""t"": ""READY"", 
                ""s"": {sequenceNumber}, 
                ""d"": {{ 
                    ""v"": {version},
                    ""user"": {{
                        ""id"": {userId}
                    }}
                }}
            }}");
            var result = await parser.Parse(payload, cancellationToken);

            result.OpCode.Should().Be(GatewayOpCode.Dispatch);
            result.SequenceNumber.Should().Be(sequenceNumber);
            result.Data.Should().BeOfType<ReadyEvent>();

            var readyEvent = result.Data.As<ReadyEvent>();
            readyEvent.GatewayVersion.Should().Be(version);
            readyEvent.User.Id.Should().Be(userId);
        }

        private Stream CreatePayloadStream(string payload)
        {
            var bytes = Encoding.UTF8.GetBytes(payload);
            return new MemoryStream(bytes);
        }

        private GeneratedMessageParser CreateParser()
        {
            var serializer = new JsonSerializer();
            return new GeneratedMessageParser(serializer);
        }
    }
}
