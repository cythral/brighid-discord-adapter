using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Models;
using Brighid.Discord.Models.Payloads;
using Brighid.Discord.Tracing;

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
            public async Task ShouldSendCreateDirectMessageChannelEndpoint(
                Snowflake recipientId,
                string traceHeader,
                [Frozen, Substitute] ITracingService tracing,
                [Frozen, Substitute] IDiscordRequestHandler handler,
                [Target] DefaultDiscordUserClient client,
                CancellationToken cancellationToken
            )
            {
                await client.CreateDirectMessageChannel(recipientId, cancellationToken);
                await handler.Received().Handle<CreateDirectMessageChannelPayload, Channel>(Is((Endpoint)UserEndpoint.CreateDirectMessageChannel), Is<CreateDirectMessageChannelPayload>(payload => payload.RecipientId == recipientId), Is((Dictionary<string, string>?)null), Is((Dictionary<string, string>?)null), Is(cancellationToken));
            }
        }
    }
}
