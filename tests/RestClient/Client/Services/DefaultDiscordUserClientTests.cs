using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Models;
using Brighid.Discord.Models.Payloads;
using Brighid.Discord.RestClient.Responses;
using Brighid.Discord.Serialization;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.RestClient.Client
{
    public class DefaultDiscordUserClientTests
    {
        [TestFixture]
        public class CreateDirectMessageChannelTests
        {
            [Test, Auto]
            public async Task ShouldListenForAResponse(
                Snowflake recipientId,
                [Frozen, Substitute] IResponseServer server,
                [Target] DefaultDiscordUserClient client,
                CancellationToken cancellationToken
            )
            {
                await client.CreateDirectMessageChannel(recipientId, cancellationToken);

                await server.Received().ListenForResponse(Any<Guid>(), Any<TaskCompletionSource<Response>>());
            }

            [Test, Auto]
            public async Task ShouldDeserializeAndReturnBody(
                Snowflake recipientId,
                [Frozen] Channel channel,
                [Frozen] Response response,
                [Frozen, Substitute] IResponseServer server,
                [Frozen, Substitute] ISerializer serializer,
                [Target] DefaultDiscordUserClient client,
                CancellationToken cancellationToken
            )
            {
                var result = await client.CreateDirectMessageChannel(recipientId, cancellationToken);

                result.Should().Be(channel);
                await serializer.Received().Deserialize<Channel>(Is(response.Body!), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldQueueARequestWithTheCorrectEndpoint(
                Snowflake recipientId,
                string requestBody,
                [Frozen] Channel channel,
                [Frozen] Response response,
                [Frozen, Substitute] IResponseServer server,
                [Frozen, Substitute] ISerializer serializer,
                [Frozen, Substitute] IRequestQueuer queuer,
                [Target] DefaultDiscordUserClient client,
                CancellationToken cancellationToken
            )
            {
                serializer.Serialize(Any<CreateDirectMessageChannelPayload>(), Any<CancellationToken>()).Returns(requestBody);

                var result = await client.CreateDirectMessageChannel(recipientId, cancellationToken);

                await serializer.Received().Serialize(
                    Is<CreateDirectMessageChannelPayload>(payload => payload.RecipientId == recipientId),
                    Is(cancellationToken)
                );

                await queuer.Received().QueueRequest(
                    Any<Request>(),
                    Is(cancellationToken)
                );

                var req = TestUtils.GetArg<Request>(queuer, nameof(IRequestQueuer.QueueRequest), 0);
                req.ResponseURL.Should().Be(server.Uri);
                req.Endpoint.Should().Be((Endpoint)UserEndpoint.CreateDirectMessageChannel);
                req.RequestBody.Should().Be(requestBody);
            }
        }
    }
}
