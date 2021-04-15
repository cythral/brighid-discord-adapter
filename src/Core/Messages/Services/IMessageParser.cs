using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Messages
{
    /// <summary>
    /// A message parser utility.
    /// </summary>
    public interface IMessageParser
    {
        /// <summary>
        /// Parses a stream of data into a gateway message, with event data included.
        /// </summary>
        /// <param name="stream">The stream with data to parse.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The parsed message.</returns>
        Task<GatewayMessage> Parse(Stream stream, CancellationToken cancellationToken);

        /// <summary>
        /// Parses a message's event data if present and creates a new message with deserialized event data.
        /// </summary>
        /// <param name="message">A message with raw event data.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>Message with event data included.</returns>
        Task<GatewayMessage> ParseEventData(GatewayMessageWithoutData message, CancellationToken cancellationToken);
    }
}
