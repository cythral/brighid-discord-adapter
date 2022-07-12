using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Messages;
using Brighid.Discord.Models;

using Microsoft.Extensions.Hosting;

namespace Brighid.Discord.Adapter.Gateway
{
    /// <summary>
    /// Service to interface with the discord gateway.
    /// </summary>
    public interface IGatewayService : IHostedService
    {
        /// <summary>
        /// Gets or sets the last sequence number received.
        /// </summary>
        int? SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the session id.
        /// </summary>
        string? SessionId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the Bot user.
        /// </summary>
        Snowflake? BotId { get; set; }

        /// <summary>
        /// Gets a value indicating the current state of the gateway.
        /// </summary>
        GatewayState State { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the gateway is waiting for heartbeat acknowledgement.
        /// </summary>
        bool AwaitingHeartbeatAcknowledgement { get; set; }

        /// <summary>
        /// Sets the gateway's ready state.
        /// </summary>
        /// <param name="ready">Whether or not the gateway is ready to process events from discord.</param>
        void SetReadyState(bool ready);

        /// <summary>
        /// Throw an exception if the gateway is not ready.
        /// </summary>
        void ThrowIfNotReady();

        /// <summary>
        /// Restarts the gateway service.
        /// </summary>
        /// <param name="resume">Whether or not to resume after reconnecting.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Restart(bool resume = true, CancellationToken cancellationToken = default);

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
        /// <returns>The resulting task.</returns>
        Task StartHeartbeat(uint heartbeatInterval);

        /// <summary>
        /// Stop sending a heartbeat to the gateway.
        /// </summary>
        /// <returns>The resulting task.</returns>
        Task StopHeartbeat();
    }
}
