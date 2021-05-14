using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Threading
{
    /// <inheritdoc />
    public class DefaultTimerFactory : ITimerFactory
    {
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTimerFactory" /> class.
        /// </summary>
        /// <param name="loggerFactory">Factory used to create loggers with.</param>
        public DefaultTimerFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public ITimer CreateTimer(AsyncTimerCallback callback, int period, string timerName)
        {
            return new Timer(callback, period, timerName, loggerFactory);
        }

        /// <inheritdoc />
        public Task CreateDelay(int millisecondsToDelay, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Delay(millisecondsToDelay, cancellationToken);
        }
    }
}
