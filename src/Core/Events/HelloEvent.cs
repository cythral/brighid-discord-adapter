using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Gateway;

namespace Brighid.Discord.Events
{
    /// <summary>
    /// Sent on connection to the websocket. Defines the heartbeat interval that the client should heartbeat to.
    /// </summary>
    [GatewayEvent(GatewayOpCode.Hello)]
    public struct HelloEvent : IGatewayEvent
    {
        /// <summary>
        /// Gets or sets the interval (in milliseconds) the client should heartbeat with.
        /// </summary>
        [JsonPropertyName("heartbeat_interval")]
        public uint HeartbeatInterval { get; set; }

        /// <inheritdoc />
        public async Task Handle(IGatewayService gateway, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask;
            gateway.StartHeartbeat(HeartbeatInterval);
        }
    }
}
