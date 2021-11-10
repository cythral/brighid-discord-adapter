using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

using AutoFixture.NUnit3;

using Brighid.Discord.Models;
using Brighid.Discord.Serialization;

using Microsoft.Extensions.Options;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Messages
{
    public class SnsMessageEmitterTests
    {
        [Test, Auto]
        public async Task EmitSerializesMessageAndPublishesToTopic(
            string message,
            string serializedMessage,
            Snowflake channelId,
            [Frozen] IAmazonSimpleNotificationService snsClient,
            [Frozen] ISerializer serializer,
            [Frozen, Options] IOptions<SnsMessageEmitterOptions> options,
            [Target] SnsMessageEmitter emitter
        )
        {
            serializer.Serialize(Any<string>()).Returns(serializedMessage);
            var cancellationToken = new CancellationToken(false);

            await emitter.Emit(message, channelId, cancellationToken);

            serializer.Received().Serialize(Is(message));
            await snsClient.Received().PublishAsync(
                Is<PublishRequest>(req =>
                    req.TopicArn == options.Value.TopicArn &&
                    req.Message == serializedMessage
                ),
                Is(cancellationToken)
            );
        }

        [Test, Auto]
        public async Task EmitPublishesMessagesWithSourceSystemAttribute(
            string message,
            string serializedMessage,
            Snowflake channelId,
            [Frozen] IAmazonSimpleNotificationService snsClient,
            [Frozen] ISerializer serializer,
            [Frozen, Options] IOptions<SnsMessageEmitterOptions> options,
            [Target] SnsMessageEmitter emitter
        )
        {
            var cancellationToken = new CancellationToken(false);

            await emitter.Emit(message, channelId, cancellationToken);

            serializer.Received().Serialize(Is(message));
            await snsClient.Received().PublishAsync(
                Is<PublishRequest>(req =>
                    req.MessageAttributes.Any(attribute =>
                        attribute.Key == "Brighid.SourceSystem" &&
                        attribute.Value.DataType == "String" &&
                        attribute.Value.StringValue == "discord"
                    )
                ),
                Is(cancellationToken)
            );
        }

        [Test, Auto]
        public async Task EmitPublishesMessagesWithSourceIdAttribute(
            string message,
            string serializedMessage,
            Snowflake channelId,
            [Frozen] IAmazonSimpleNotificationService snsClient,
            [Frozen] ISerializer serializer,
            [Frozen, Options] IOptions<SnsMessageEmitterOptions> options,
            [Target] SnsMessageEmitter emitter
        )
        {
            var cancellationToken = new CancellationToken(false);

            await emitter.Emit(message, channelId, cancellationToken);

            serializer.Received().Serialize(Is(message));
            await snsClient.Received().PublishAsync(
                Is<PublishRequest>(req =>
                    req.MessageAttributes.Any(attribute =>
                        attribute.Key == "Brighid.SourceId" &&
                        attribute.Value.DataType == "String" &&
                        attribute.Value.StringValue == channelId
                    )
                ),
                Is(cancellationToken)
            );
        }
    }
}
