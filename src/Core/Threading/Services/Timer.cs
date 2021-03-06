using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Threading
{
    /// <summary>
    /// Callback that the timer invokes on an interval.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The resulting task.</returns>
    public delegate Task AsyncTimerCallback(CancellationToken cancellationToken = default);

    /// <summary>Function that occurs when a task stops.</summary>
    /// <returns>The resulting task.</returns>
    public delegate Task OnUnexpectedTimerStop();

    /// <inheritdoc />
    public class Timer : ITimer
    {
        private readonly AsyncTimerCallback callback;
        private readonly int period;
        private readonly ILogger logger;
        private readonly string timerName;
        private TaskCompletionSource? startPromise;
        private TaskCompletionSource? stopPromise;
        private CancellationTokenSource? cancellationTokenSource;
        private CancellationToken cancellationToken;
        private Thread? thread;

        /// <summary>
        /// Initializes a new instance of the <see cref="Timer" /> class.
        /// </summary>
        /// <param name="callback">The callback to call with the given period.</param>
        /// <param name="period">The period at which to invoke the callback.</param>
        /// <param name="timerName">Name of the timer.</param>
        /// <param name="loggerFactory">Factory used to create loggers.</param>
        public Timer(
            AsyncTimerCallback callback,
            int period,
            string timerName,
            ILoggerFactory loggerFactory
        )
        {
            this.period = period;
            this.callback = callback;
            this.timerName = timerName;
            logger = loggerFactory.CreateLogger(timerName);
        }

        /// <inheritdoc />
        public bool StopOnException { get; set; }

        /// <inheritdoc />
        public OnUnexpectedTimerStop? OnUnexpectedStop { get; set; }

        /// <inheritdoc />
        public async Task Start()
        {
            logger.LogInformation("Starting Timer.");
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            startPromise = new TaskCompletionSource();
            stopPromise = new TaskCompletionSource();
            thread = new Thread(RunAsync);
            thread.Start();
            await startPromise.Task;
        }

        /// <inheritdoc />
        public async Task Stop()
        {
            logger.LogInformation("Stopping Timer.");
            cancellationTokenSource?.Cancel();
            await stopPromise!.Task;
        }

        private async void RunAsync()
        {
            using var scope = logger.BeginScope("{@timerName}", timerName);
            startPromise?.TrySetResult();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (period > 0)
                    {
                        await Task.Delay(period, cancellationToken);
                    }

                    await callback(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("Timer canceled, shutting down gracefully.");
                }
                catch (Exception exception)
                {
                    logger.LogError("Received exception: {@exception}", exception);

                    if (StopOnException)
                    {
                        if (OnUnexpectedStop != null)
                        {
                            scope.Dispose();
                            stopPromise?.TrySetResult();
                            await OnUnexpectedStop();
                        }

                        break;
                    }
                }
            }

            logger.LogInformation("Timer stopped.");
            stopPromise?.TrySetResult();
        }
    }
}
