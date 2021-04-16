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
        /// <param name="workerName">Name of the worker.</param>
        /// <param name="logger">Logger used to log information to a destination.</param>
        public WorkerThread(Func<Task> runAsync, string workerName, ILogger logger)
        {
            this.runAsync = runAsync;
            this.logger = logger;
            Name = workerName;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public void Start(CancellationTokenSource cancellationTokenSource)
        {
            logger.LogInformation("{@workerName} Starting Worker Thread.", Name);
            this.cancellationTokenSource = cancellationTokenSource;
            thread = new Thread(Run);
            thread.Start();
        }

        /// <inheritdoc />
        public void Stop()
        {
            logger.LogInformation("{@workerName} Stopping Worker Thread.", Name);
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

        private void Run()
        {
            try
            {
                logger.LogInformation("{@workerName} Worker Thread Started.", Name);
                runAsync().GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("{@workerName} Worker Thread Stopped Gracefully.", Name);
            }
            catch (Exception exception)
            {
                logger.LogError("{@workerName} Raised exception {@exception}", Name, exception);
            }
        }
    }
}
