using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.GatewayAdapter.Gateway
{
    /// <summary>
    /// Channel that different threads can use to communicate with each other.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to write.</typeparam>
    public interface IChannel<TMessage>
    {
        /// <summary>
        /// Waits until a message is ready to be read from the channel.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>True if there is a message available to be read or false if not.</returns>
        ValueTask<bool> WaitToRead(CancellationToken cancellationToken = default);

        /// <summary>
        /// Read a message from the channel.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The message read from the channel.</returns>
        ValueTask<TMessage> Read(CancellationToken cancellationToken = default);

        /// <summary>
        /// Write a message to the channel.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        ValueTask Write(TMessage message, CancellationToken cancellationToken);
    }
}
