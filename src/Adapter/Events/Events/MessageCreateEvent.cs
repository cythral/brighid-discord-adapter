using System.Text.Json.Serialization;

using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Event sent when a message is created in a guild.
    /// </summary>
    [GatewayEvent(GatewayOpCode.Dispatch, "MESSAGE_CREATE")]
    [JsonConverter(typeof(IMessageEventConverter<MessageCreateEvent>))]
    public struct MessageCreateEvent : IGatewayEvent, IMessageEvent
    {
        /// <summary>
        /// Gets or sets the event message.
        /// </summary>
        public Message Message { get; set; }

        /// <summary>
        /// Gets a value indicating whether the message originates from a bot user or not.
        /// </summary>
        public bool IsBotMessage => Message.Author.IsBot ?? false;
    }
}
