using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Models;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.RestClient.Client
{
    public class DefaultDiscordGatewayClientTests
    {
        [TestFixture]
        [Category("Unit")]
        public class GetGatewayInfoTests
        {
            [Test, Auto]
            public async Task ShouldReturnGatewayInfo(
                Snowflake channelId,
                [Frozen] GatewayInfo gatewayInfo,
                [Frozen, Substitute] IDiscordRequestHandler handler,
                [Target] DefaultDiscordGatewayClient client,
                CancellationToken cancellationToken
            )
            {
                var result = await client.GetGatewayInfo(cancellationToken);

                result.Should().Be(gatewayInfo);
                await handler.Received().Handle<GatewayInfo>(
                    endpoint: Is((Endpoint)GatewayEndpoint.GetGateway),
                    parameters: Is((Dictionary<string, string>?)null),
                    headers: Is((Dictionary<string, string>?)null),
                    cancellationToken: Is(cancellationToken)
                );
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class GetGatewayBotInfoTests
        {
            [Test, Auto]
            public async Task ShouldReturnGatewayBotInfo(
                Snowflake channelId,
                [Frozen] GatewayBotInfo gatewayBotInfo,
                [Frozen, Substitute] IDiscordRequestHandler handler,
                [Target] DefaultDiscordGatewayClient client,
                CancellationToken cancellationToken
            )
            {
                var result = await client.GetGatewayBotInfo(cancellationToken);

                result.Should().Be(gatewayBotInfo);
                await handler.Received().Handle<GatewayBotInfo>(
                    endpoint: Is((Endpoint)GatewayEndpoint.GetGatewayBot),
                    parameters: Is((Dictionary<string, string>?)null),
                    headers: Is((Dictionary<string, string>?)null),
                    cancellationToken: Is(cancellationToken)
                );
            }
        }
    }
}
