using System.Text.Json.Serialization;

using Brighid.Discord.Gateway;

namespace Brighid.Discord.Messages
{
    /// <summary>
    /// Represents a message sent directly to/from the gateway.
    /// </summary>
    public struct GatewayMessage
    {
        /// <summary>
        /// Gets or sets the opcode for the payload.
        /// </summary>
        [JsonPropertyName("op")]
        public GatewayOpCode OpCode { get; set; }

        /// <summary>
        /// Gets or sets the sequence number, used for resuming sessions and heartbeats.
        /// </summary>
        [JsonPropertyName("s")]
        public int? SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the event name for this payload.
        /// </summary>
        [JsonPropertyName("t")]
        public string? EventName { get; set; }

        /// <summary>
        /// Gets or sets the event data for this payload.
        /// </summary>
        [JsonPropertyName("d")]
        public IGatewayEvent? Data { get; set; }
    }
}
