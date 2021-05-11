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
    }
}
