using System.Text.Json.Serialization;

using Destructurama.Attributed;

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Used to replay missed events when a disconnected client resumes.
    /// </summary>
    [GatewayEvent(GatewayOpCode.Resume)]
    public struct ResumeEvent : IGatewayEvent
    {
        /// <summary>
        /// Gets or sets the session token.
        /// </summary>
        [NotLogged]
        [JsonPropertyName("token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the session id.
        /// </summary>
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the last sequence number received.
        /// </summary>
        [JsonPropertyName("seq")]
        public int? SequenceNumber { get; set; }
    }
}
