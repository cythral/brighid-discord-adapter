using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Threading
{
    /// <summary>
    /// Predicate used to filter out a message from the queue.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to filter.</typeparam>
    /// <param name="message">The message to check if it needs to be filtered out.</param>
    /// <returns>True if the message should be kept, or false if not.</returns>
    public delegate ValueTask<bool> FilterPredicate<TMessage>(TMessage message);

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
        /// Waits until the channel is ready to be written to.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        ValueTask<bool> WaitToWrite(CancellationToken cancellationToken = default);

        /// <summary>
        /// Write a message to the channel.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        ValueTask Write(TMessage message, CancellationToken cancellationToken);

        /// <summary>
        /// Reads pending messages in the queue.
        /// </summary>
        /// <param name="max">Maximum number of messages to read from the queue.</param>
        /// <param name="filter">Predicate used to filter out unwanted messages.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The resulting task.</returns>
        ValueTask<IEnumerable<TMessage>> ReadPending(uint max, FilterPredicate<TMessage>? filter = null, CancellationToken cancellationToken = default);
    }
}
