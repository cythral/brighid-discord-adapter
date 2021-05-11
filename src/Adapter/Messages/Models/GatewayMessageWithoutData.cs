using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Adapter.Messages
{
    /// <summary>
    /// Represents a message sent directly to/from the gateway.
    /// </summary>
    public class GatewayMessageWithoutData
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
        /// Gets or sets the extension data.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();

        /// <summary>
        /// Constructs a new gateway message from this one with data included.
        /// </summary>
        /// <param name="data">The data to add to the message.</param>
        /// <returns>A new gateway message with data included.</returns>
        public GatewayMessage WithData(IGatewayEvent? data)
        {
            return new GatewayMessage
            {
                OpCode = OpCode,
                SequenceNumber = SequenceNumber,
                EventName = EventName,
                Data = data,
            };
        }

        /// <summary>
        /// Deconstructs the gateway message.
        /// </summary>
        /// <param name="opCode">The gateway message's op code.</param>
        /// <param name="eventName">The event name.</param>
        public void Deconstruct(out GatewayOpCode opCode, out string? eventName)
        {
            opCode = OpCode;
            eventName = EventName;
        }
    }
}
