using System.Text.Json.Serialization;

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// The reconnect event is dispatched when a client should reconnect to the gateway (and resume their existing session, if they have one). This event usually occurs during deploys to migrate sessions gracefully off old hosts.
    /// </summary>
    [GatewayEvent(GatewayOpCode.InvalidSession)]
    [JsonConverter(typeof(InvalidSessionEventConverter))]
    public struct InvalidSessionEvent : IGatewayEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSessionEvent" /> struct.
        /// </summary>
        /// <param name="isResumable">Value indicating whether the session is resumable.</param>
        public InvalidSessionEvent(bool isResumable)
        {
            IsResumable = isResumable;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the connection is resumable or not.
        /// </summary>
        public bool IsResumable { get; set; }
    }
}
