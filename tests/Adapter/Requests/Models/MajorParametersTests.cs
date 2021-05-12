using System.Collections.Generic;

using FluentAssertions;

using NUnit.Framework;

namespace Brighid.Discord.Adapter.Requests
{
    public class MajorParametersTests
    {
        [Test, Auto]
        public void ShouldAddChannelIdToTheValue(
            string channelId,
            string messageId
        )
        {
            var parameters = new Dictionary<string, string>
            {
                ["channel.id"] = channelId,
                ["message.id"] = messageId,
            };

            var majorParameters = new MajorParameters(parameters);
            majorParameters.Should().Be($"/{channelId}");
        }

        [Test, Auto]
        public void ShouldAddGuildIdToTheValue(
            string guildId,
            string userId
        )
        {
            var parameters = new Dictionary<string, string>
            {
                ["guild.id"] = guildId,
                ["user.id"] = userId,
            };

            var majorParameters = new MajorParameters(parameters);
            majorParameters.Should().Be($"/{guildId}");
        }

        [Test, Auto]
        public void ShouldAddWebhookIdAndTokenToTheValue(
            string webhookId,
            string webhookToken
        )
        {
            var parameters = new Dictionary<string, string>
            {
                ["webhook.id"] = webhookId,
                ["webhook.token"] = webhookToken,
            };

            var majorParameters = new MajorParameters(parameters);
            majorParameters.Should().Be($"/{webhookId}/{webhookToken}");
        }
    }
}
