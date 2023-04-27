using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Management;
using Brighid.Discord.Models;
using Brighid.Discord.RestClient.Client;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Gateway
{
    [TestFixture]
    [Category("Unit")]
    public class DefaultGatewayMetadataServiceTests
    {
        [TestFixture]
        [Category("Unit")]
        public class GetGatewayUrlTests
        {
            [Test, Auto]
            public async Task GetShouldRetrieveGatewayUrlFromApiIfNotSet(
                string url,
                [Frozen] IDiscordApiInfoService discordApiInfoService,
                [Frozen] IDiscordGatewayClient gatewayClient,
                [Target] DefaultGatewayMetadataService service,
                CancellationToken cancellationToken
            )
            {
                gatewayClient.GetGatewayBotInfo(Any<CancellationToken>()).Returns(new GatewayBotInfo
                {
                    Url = $"wss://{url}",
                });

                var result = await service.GetGatewayUrl(cancellationToken);

                result.Scheme.Should().Be("wss");
                result.Host.Should().Be(url);

                var queryParams = HttpUtility.ParseQueryString(result.Query);
                queryParams["v"].Should().Be(discordApiInfoService.ApiVersion);
                queryParams["encoding"].Should().Be("json");

                await gatewayClient.Received().GetGatewayBotInfo(Is(cancellationToken));
            }

            [Test, Auto]
            public async Task GetShouldCacheValues(
                string url,
                [Frozen] IDiscordApiInfoService discordApiInfoService,
                [Frozen] IDiscordGatewayClient gatewayClient,
                [Target] DefaultGatewayMetadataService service,
                CancellationToken cancellationToken
            )
            {
                gatewayClient.GetGatewayBotInfo(Any<CancellationToken>()).Returns(new GatewayBotInfo
                {
                    Url = $"wss://{url}",
                });

                _ = await service.GetGatewayUrl(cancellationToken);
                _ = await service.GetGatewayUrl(cancellationToken);

                await gatewayClient.Received(1).GetGatewayBotInfo(Is(cancellationToken));
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class SetGatewayUrlTests
        {
            [Test, Auto]
            public async Task SetShouldOverwritePreviousValue(
                Uri url,
                [Frozen] IDiscordApiInfoService discordApiInfoService,
                [Frozen] IDiscordGatewayClient gatewayClient,
                [Target] DefaultGatewayMetadataService service,
                CancellationToken cancellationToken
            )
            {
                _ = await service.GetGatewayUrl(cancellationToken);
                service.SetGatewayUrl(url);

                var result = await service.GetGatewayUrl(cancellationToken);
                result.Scheme.Should().Be(url.Scheme);
                result.Host.Should().Be(url.Host);

                var queryParams = HttpUtility.ParseQueryString(result.Query);
                queryParams["v"].Should().Be(discordApiInfoService.ApiVersion);
                queryParams["encoding"].Should().Be("json");
            }

            [Test, Auto]
            public async Task SetWithNullShouldResetTheValue(
                Uri url,
                [Frozen] IDiscordApiInfoService discordApiInfoService,
                [Frozen] IDiscordGatewayClient gatewayClient,
                [Target] DefaultGatewayMetadataService service,
                CancellationToken cancellationToken
            )
            {
                var initialUrl = await service.GetGatewayUrl(cancellationToken);
                service.SetGatewayUrl(url);
                service.SetGatewayUrl(null);
                var result = await service.GetGatewayUrl(cancellationToken);

                result.Should().Be(initialUrl);
            }
        }
    }
}
