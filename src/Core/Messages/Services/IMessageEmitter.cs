using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Messages
{
    /// <summary>
    /// An object cabable of emitting an event to a destination, such as SNS.
    /// </summary>
    public interface IMessageEmitter
    {
        /// <summary>
        ///  Emits a message to a destination.
        /// </summary>
        /// <typeparam name="TMessageType">The type of message to emit.</typeparam>
        /// <param name="message">The message to emit.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The resulting task.</returns>
        Task Emit<TMessageType>(TMessageType message, CancellationToken cancellationToken = default);
    }
}
