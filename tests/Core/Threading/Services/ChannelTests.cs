using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

namespace Brighid.Discord.Threading
{
    public class ChannelTests
    {
        [TestFixture]
        public class ReadAllPendingTests
        {
            [Test, Auto]
            public async Task ShouldReadAllMessagesInTheQueue(
                string message1,
                string message2,
                [Target] Channel<string> channel,
                CancellationToken cancellationToken
            )
            {
                await channel.Write(message1, cancellationToken);
                await channel.Write(message2, cancellationToken);

                var result = await channel.ReadPending(10, cancellationToken: cancellationToken);

                result.Should().Contain(message1);
                result.Should().Contain(message2);
            }

            [Test, Auto]
            public async Task ShouldFilterOutUnwantedMessagesInQueue(
                string message1,
                string message2,
                [Target] Channel<string> channel,
                CancellationToken cancellationToken
            )
            {
                await channel.Write(message1, cancellationToken);
                await channel.Write(message2, cancellationToken);

                var result = await channel.ReadPending(
                    10,
                    (message) => new ValueTask<bool>(message != message2),
                    cancellationToken
                );

                result.Should().Contain(message1);
                result.Should().NotContain(message2);
            }

            [Test, Auto]
            public async Task ShouldCapOutMessagesAtGivenMaximum(
                string message1,
                string message2,
                [Target] Channel<string> channel,
                CancellationToken cancellationToken
            )
            {
                await channel.Write(message1, cancellationToken);
                await channel.Write(message2, cancellationToken);

                var result = await channel.ReadPending(1, cancellationToken: cancellationToken);

                result.Should().Contain(message1);
                result.Should().NotContain(message2);
            }
        }
    }
}
