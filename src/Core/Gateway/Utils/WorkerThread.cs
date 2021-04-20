using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Gateway
{
    /// <inheritdoc />
    public class WorkerThread : IWorkerThread
    {
        private readonly ILogger logger;
        private readonly Func<Task> runAsync;
        private CancellationTokenSource? cancellationTokenSource;
        private Thread? thread;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerThread" /> class.
        /// </summary>
        /// <param name="runAsync">The async function to run on its own thread.</param>
        /// <param name="workerName">The name of the worker thread.</param>
        /// <param name="logger">Logger used to log information to a destination.</param>
        public WorkerThread(
            Func<Task> runAsync,
            string workerName,
            ILogger logger
        )
        {
            this.runAsync = runAsync;
            this.logger = logger;
            Name = workerName;
        }

        /// <inheritdoc />
        public OnUnexpectedStop? OnUnexpectedStop { get; set; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public void Start(CancellationTokenSource cancellationTokenSource)
        {
            logger.LogInformation("Starting Worker Thread.");
            this.cancellationTokenSource = cancellationTokenSource;
            thread = new Thread(Run);
            thread.Start();
        }

        /// <inheritdoc />
        public void Stop()
        {
            logger.LogInformation("Stopping Worker Thread.");
            if (thread == null)
            {
                return;
            }

            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }

            thread = null;
        }

        private async void Run()
        {
            try
            {
                logger.LogInformation("Worker Thread Started.");
                await runAsync();
                throw new OperationCanceledException();
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Worker Thread Stopped Gracefully.");
            }
            catch (Exception exception)
            {
                logger.LogError("Raised exception {@exception}", exception);

                if (OnUnexpectedStop != null)
                {
                    await OnUnexpectedStop();
                }
            }
        }
    }
}
