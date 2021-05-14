using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Threading
{
    /// <summary>
    /// Factory to create timers with.
    /// </summary>
    public interface ITimerFactory
    {
        /// <summary>
        /// Create a new timer with the given callback and period.
        /// </summary>
        /// <param name="callback">The callback to call on a period with.</param>
        /// <param name="period">The amount of time in ms to wait between invocations.</param>
        /// <param name="timerName">Name of the timer.</param>
        /// <returns>The resulting timer.</returns>
        ITimer CreateTimer(AsyncTimerCallback callback, int period, string timerName);

        /// <summary>
        /// Create a task that completes after a delay.
        /// </summary>
        /// <param name="millisecondsToDelay">Number of milliseconds to delay for.</param>
        /// <param name="cancellationToken">Token used to cancel the operation with.</param>
        /// <returns>The resulting task.</returns>
        Task CreateDelay(int millisecondsToDelay, CancellationToken cancellationToken = default);
    }
}
