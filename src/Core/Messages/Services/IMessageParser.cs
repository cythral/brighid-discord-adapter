using System.Text.Json;

namespace Brighid.Discord.Messages
{
    /// <summary>
    /// A message parser utility.
    /// </summary>
    public interface IMessageParser
    {
        /// <summary>
        /// Parses a message's event data if present and creates a new message with deserialized event data.
        /// </summary>
        /// <param name="message">A message with raw event data.</param>
        /// <param name="options">Json Serializer Options to use when reading data.</param>
        /// <returns>Message with event data included.</returns>
        GatewayMessage ParseEventData(GatewayMessageWithoutData message, JsonSerializerOptions options);
    }
}
