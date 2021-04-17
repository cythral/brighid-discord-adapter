using System.Text.Json.Serialization;

using Brighid.Discord.Models;
using Brighid.Discord.Serialization;

namespace Brighid.Discord.Events
{
    /// <summary>
    /// Event sent when a message is created in a guild.
    /// </summary>
    [GatewayEvent(GatewayOpCode.Dispatch, "MESSAGE_CREATE")]
    [JsonConverter(typeof(IMessageEventConverterFactory))]
    public struct MessageCreateEvent : IGatewayEvent, IMessageEvent
    {
        /// <summary>
        /// Gets or sets the event message.
        /// </summary>
        public Message Message { get; set; }
    }
}
