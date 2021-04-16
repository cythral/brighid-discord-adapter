using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Messages;

namespace Brighid.Discord.Gateway
{
    /// <summary>
    /// Service to interface with the discord gateway.
    /// </summary>
    public interface IGatewayService
    {
        /// <summary>
        /// Start the gateway service.
        /// </summary>
        /// <param name="cancellationTokenSource">Source token used to cancel the worker's thread.</param>
        void Start(CancellationTokenSource cancellationTokenSource);

        /// <summary>
        /// Stop the gateway service.
        /// </summary>
        void Stop();

        /// <summary>
        /// Sends a message to the gateway.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Send(GatewayMessage message, CancellationToken cancellationToken = default);
    }
}
