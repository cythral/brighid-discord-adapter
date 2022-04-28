using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Models;
using Brighid.Discord.Models.Payloads;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.RestClient.Client
{
    public class DefaultDiscordChannelClientTests
    {
        [TestFixture]
        public class CreateMessageTests
        {
            [Test, Auto]
            public async Task ShouldCreateAMessageInTheCorrectChannel(
                Snowflake channelId,
                CreateMessagePayload payload,
                [Frozen, Substitute] IDiscordRequestHandler handler,
                [Target] DefaultDiscordChannelClient client,
                CancellationToken cancellationToken
            )
            {
                await client.CreateMessage(channelId, payload, cancellationToken);

                await handler.Received().Handle(
                    endpoint: Is((Endpoint)ChannelEndpoint.CreateMessage),
                    request: Is(payload),
                    parameters: Is<Dictionary<string, string>>(parameters => parameters["channel.id"] == channelId),
                    headers: Is((Dictionary<string, string>?)null),
                    cancellationToken: Is(cancellationToken)
                );
            }
        }
    }
}
