using System;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Adapter.Gateway
{
    /// <summary>
    /// State of the gateway.
    /// </summary>
    [Flags]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GatewayState
    {
        /// <summary>
        /// State when the gateway is stopped.
        /// </summary>
        Stopped,

        /// <summary>
        /// State when the gateway is running.
        /// </summary>
        Running,

        /// <summary>
        /// State when the initial handshake with discord is complete.
        /// </summary>
        Ready,
    }
}
