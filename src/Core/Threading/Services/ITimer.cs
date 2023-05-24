using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Threading
{
    /// <summary>
    /// Timer that does work every few seconds.
    /// </summary>
    public interface ITimer
    {
        /// <summary>
        /// Gets or sets a value indicating whether the timer should stop on exception.
        /// </summary>
        bool StopOnException { get; set; }

        /// <summary>
        /// Gets or sets a handler that is invoked when the timer stops unexpectedly.
        /// This will not be invoked if <see cref="StopOnException" /> is set to false.
        /// </summary>
        OnUnexpectedTimerStop? OnUnexpectedStop { get; set; }

        /// <summary>
        /// Start the timer.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Start(CancellationToken cancellationToken);

        /// <summary>
        /// Stop the timer.
        /// </summary>
        /// <returns>The resulting task.</returns>
        Task Stop();
    }
}
