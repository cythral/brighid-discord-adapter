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
        /// Gets or sets the last sequence number received.
        /// </summary>
        int? SequenceNumber { get; set; }

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

        /// <summary>
        /// Start sending a heartbeat every <paramref name="heartbeatInterval"/> milliseconds to the gateway.
        /// </summary>
        /// <param name="heartbeatInterval">The interval to heartbeat at.</param>
        void StartHeartbeat(uint heartbeatInterval);

        /// <summary>
        /// Stop sending a heartbeat to the gateway.
        /// </summary>
        void StopHeartbeat();
    }
}
