using System.Threading.Tasks;

namespace Brighid.Discord.Threading
{
    /// <summary>
    /// Timer that does work every few seconds.
    /// </summary>
    public interface ITimer
    {
        /// <summary>
        /// Start the timer.
        /// </summary>
        /// <returns>The resulting task.</returns>
        Task Start();

        /// <summary>
        /// Stop the timer.
        /// </summary>
        /// <returns>The resulting task.</returns>
        Task Stop();
    }
}
