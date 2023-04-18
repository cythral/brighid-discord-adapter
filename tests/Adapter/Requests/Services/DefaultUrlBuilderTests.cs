using System;

using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Management;
using Brighid.Discord.Models;

using FluentAssertions;

using NUnit.Framework;

namespace Brighid.Discord.Adapter.Requests
{
    public class DefaultUrlBuilderTests
    {
        [TestFixture]
        public class BuildFromRequestTests
        {
            [Test, Auto]
            public void ShouldThrowIfGivenAnUnhandledEndpoint(
                Request request,
                [Target] DefaultUrlBuilder builder
            )
            {
                var endpoint = (ChannelEndpoint)9999999;
                request.Endpoint = endpoint;

                Action func = () => builder.BuildFromRequest(request);

                func.Should().Throw<UnhandledEndpointException>()
                .And.Endpoint.Should().Be((Endpoint)endpoint);
            }

            [Test, Auto]
            public void ShouldBuildChannelCreateMessageEndpoints(
                string channelId,
                Request request,
                Uri baseUrl,
                [Frozen] RequestOptions options,
                [Frozen] IDiscordApiInfoService discordApiInfoService,
                [Target] DefaultUrlBuilder builder
            )
            {
                request.Endpoint = ChannelEndpoint.CreateMessage;
                request.Parameters["channel.id"] = channelId;

                var result = builder.BuildFromRequest(request);
                result.Should().Be($"{discordApiInfoService.ApiBaseUrl}/channels/{channelId}/messages");
            }

            [Test, Auto]
            public void ShouldThrowIfChannelIdIsMissingFromChannelCreateMessageRequest(
                Request request,
                [Target] DefaultUrlBuilder builder
            )
            {
                request.Endpoint = ChannelEndpoint.CreateMessage;

                Action func = () => builder.BuildFromRequest(request);

                func.Should().Throw<MissingParameterException>()
                .And.Parameter.Should().Be("channel.id");
            }
        }
    }
}
