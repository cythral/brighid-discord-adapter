using System.Text.Json.Serialization;

using Brighid.Discord.Models;

namespace Brighid.Discord.AlarmNotifier
{
    /// <summary>
    /// Payload sent when executing a payload.
    /// </summary>
    public struct ExecuteWebhookPayload
    {
        /// <summary>
        /// Gets or sets the embeds to include in the webhook message.
        /// </summary>
        [JsonPropertyName("embeds")]
        public Embed[]? Embeds { get; set; }
    }
}
