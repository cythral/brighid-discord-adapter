using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter
{
    /// <summary>
    /// Events that can be logged.
    /// </summary>
    public static class LogEvents
    {
        /// <summary>
        /// Event for when an invalid session event is received.
        /// </summary>
        public static readonly EventId InvalidSessionEvent = new(0x00000001, nameof(InvalidSessionEvent));

        /// <summary>
        /// Event for when a reconnect event is received.
        /// </summary>
        public static readonly EventId ReconnectEvent = new(0x00000002, nameof(ReconnectEvent));

        /// <summary>
        /// Event for when a resumed event is received.
        /// </summary>
        public static readonly EventId ResumedEvent = new(0x00000003, nameof(ResumedEvent));

        /// <summary>
        /// Event for when a rest api call is rate limited.
        /// </summary>
        public static readonly EventId RestApiRateLimited = new(0x00000004, nameof(RestApiRateLimited));

        /// <summary>
        /// Event for when a rest api call fails.
        /// </summary>
        public static readonly EventId RestApiFailed = new(0x00000005, nameof(RestApiFailed));

        /// <summary>
        /// Event for when the gateway is restarted.
        /// </summary>
        public static readonly EventId GatewayRestarted = new(0x00000005, nameof(GatewayRestarted));
    }
}
