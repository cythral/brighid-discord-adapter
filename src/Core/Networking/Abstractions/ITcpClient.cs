using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Networking
{
    /// <summary>
    /// A Client that connected over TCP, capable of sending and receiving messages.
    /// </summary>
    public interface ITcpClient : IDisposable
    {
        /// <summary>
        /// Gets the stream associated with the TCP Client.
        /// </summary>
        /// <returns>The client's stream.</returns>
        Stream GetStream();

        /// <summary>
        /// Write a string to the TCP Client's stream.
        /// </summary>
        /// <param name="payload">The payload to write.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Write(string payload, CancellationToken cancellationToken = default);

        /// <summary>
        /// Closes the TCP listener's connection.
        /// </summary>
        void Close();
    }
}
