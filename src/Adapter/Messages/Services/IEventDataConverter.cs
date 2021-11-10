using System.Text.Json;

namespace Brighid.Discord.Adapter.Messages
{
    /// <summary>
    /// A message parser utility.
    /// </summary>
    public interface IEventDataConverter
    {
        /// <summary>
        /// Parses a message's event data if present and creates a new message with deserialized event data.
        /// </summary>
        /// <param name="message">A message with raw event data.</param>
        /// <returns>Message with event data included.</returns>
        GatewayMessage ParseEventData(GatewayMessageWithoutData message);

        /// <summary>
        /// Parses a message's event data if present and creates a new message with deserialized event data.
        /// </summary>
        /// <param name="writer">JSON writer to write data with.</param>
        /// <param name="data">Event data to write.</param>
        void WriteEventData(Utf8JsonWriter writer, IGatewayEvent data);
    }
}
