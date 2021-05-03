using System.Text.Json.Serialization;

namespace Brighid.Discord.GatewayAdapter.Events
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
    }
}
