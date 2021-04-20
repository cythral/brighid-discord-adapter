using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Gateway
{
    /// <summary>Function that occurs when a task stops.</summary>
    /// <returns>The resulting task.</returns>
    public delegate Task OnUnexpectedStop();

    /// <summary>
    /// A worker thread.
    /// </summary>
    public interface IWorkerThread
    {
        /// <summary>
        /// Gets or sets the event handler that is triggered on an unexpected stop.
        /// </summary>
        OnUnexpectedStop? OnUnexpectedStop { get; set; }

        /// <summary>
        /// Gets the name of the worker thread.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Start the worker thread.
        /// </summary>
        /// <param name="cancellationTokenSource">Source token used to cancel the worker's thread.</param>
        void Start(CancellationTokenSource cancellationTokenSource);

        /// <summary>
        /// Stop the worker thread.
        /// </summary>
        void Stop();
    }
}
