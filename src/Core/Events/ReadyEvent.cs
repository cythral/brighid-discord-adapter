using System.Text.Json.Serialization;

using Brighid.Discord.Gateway;
using Brighid.Discord.Models;

namespace Brighid.Discord.Events
{
    /// <summary>
    /// The ready event is dispatched when a client has completed the initial handshake with the gateway (for new sessions).
    /// The ready event can be the largest and most complex event the gateway will send, as it contains all the state required for a client to begin interacting with the rest of the platform.
    /// </summary>
    [GatewayEvent(GatewayOpCode.Dispatch, "READY")]
    public struct ReadyEvent : IGatewayEvent
    {
        /// <summary>
        /// Gets or sets the gateway version.
        /// </summary>
        [JsonPropertyName("v")]
        public uint GatewayVersion { get; set; }

        /// <summary>
        /// Gets or sets information about the user including email.
        /// </summary>
        [JsonPropertyName("user")]
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the guilds the user is in.
        /// </summary>
        [JsonPropertyName("guilds")]
        public Guild[] Guilds { get; set; }

        /// <summary>
        /// Gets or sets the session id used for resuming connections.
        /// </summary>
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the shard information associated with this session, if sent when identifying.
        /// </summary>
        [JsonPropertyName("shard")]
        public int[] Shard { get; set; }

        /// <summary>
        /// Gets or sets the application, containing id and flags.
        /// </summary>
        [JsonPropertyName("application")]
        public Application Application { get; set; }
    }
}
