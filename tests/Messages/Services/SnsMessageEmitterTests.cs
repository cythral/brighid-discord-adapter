using System.Threading;
using System.Threading.Tasks;

using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

using AutoFixture.NUnit3;

using Brighid.Discord.GatewayAdapter.Serialization;

using Microsoft.Extensions.Options;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.GatewayAdapter.Messages
{
    public class SnsMessageEmitterTests
    {
        [Test, Auto]
        public async Task EmitSerializesMessageAndPublishesToTopic(
            string message,
            string serializedMessage,
            [Frozen] IAmazonSimpleNotificationService snsClient,
            [Frozen] ISerializer serializer,
            [Frozen, Options] IOptions<SnsMessageEmitterOptions> options,
            [Target] SnsMessageEmitter emitter
        )
        {
            serializer.Serialize(Any<string>(), Any<CancellationToken>()).Returns(serializedMessage);
            var cancellationToken = new CancellationToken(false);

            await emitter.Emit(message, cancellationToken);

            await serializer.Received().Serialize(Is(message), Is(cancellationToken));
            await snsClient.Received().PublishAsync(
                Is<PublishRequest>(req =>
                    req.TopicArn == options.Value.TopicArn &&
                    req.Message == serializedMessage
                ),
                Is(cancellationToken)
            );
        }
    }
}
