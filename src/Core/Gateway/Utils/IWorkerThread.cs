using System.Threading;

namespace Brighid.Discord.Gateway
{
    /// <summary>
    /// A worker thread.
    /// </summary>
    public interface IWorkerThread
    {
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
