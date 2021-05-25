using System.Text.Json.Serialization;

namespace Brighid.Discord.Models.Payloads
{
    /// <summary>
    /// Payload sent to create a direct message channel.
    /// </summary>
    public struct CreateDirectMessageChannelPayload
    {
        /// <summary>
        /// Gets or sets the ID of the recipient.
        /// </summary>
        [JsonPropertyName("recipient_id")]
        public Snowflake RecipientId { get; set; }
    }
}
