using System.IO;

namespace Brighid.Discord.RestClient.Responses
{
    /// <summary>
    /// A Client that connected over TCP, capable of sending and receiving messages.
    /// </summary>
    public interface ITcpClient
    {
        /// <summary>
        /// Gets the stream associated with the TCP Client.
        /// </summary>
        /// <returns>The client's stream.</returns>
        Stream GetStream();
    }
}
