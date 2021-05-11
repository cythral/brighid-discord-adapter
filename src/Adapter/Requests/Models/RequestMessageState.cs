namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// State of a RequestMessage.
    /// </summary>
    public enum RequestMessageState
    {
        /// <summary>
        /// The message has been taken off the queue and is in-flight.
        /// </summary>
        InFlight,

        /// <summary>
        /// Processing the message failed.
        /// </summary>
        Failed,

        /// <summary>
        /// Processing the message succeeded.
        /// </summary>
        Succeeded,
    }
}
