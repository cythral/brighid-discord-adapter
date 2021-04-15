namespace Brighid.Discord
{
    /// <summary>
    /// All gateway events in Discord are tagged with an opcode that denotes the payload type. Your connection to our gateway may also sometimes close. When it does, you will receive a close code that tells you what happened.
    /// </summary>
    public enum GatewayOpCode
    {
        /// <summary>
        /// An event was dispatched.
        /// </summary>
        Dispatch = 0,

        /// <summary>
        /// Fired periodically by the client to keep the connection alive.
        /// </summary>
        Heartbeat = 1,

        /// <summary>
        /// Starts a new session during the initial handshake.
        /// </summary>
        Identify = 2,

        /// <summary>
        /// Update the client's presence.
        /// </summary>
        PresenceUpdate = 3,

        /// <summary>
        /// Used to join/leave or move between voice channels.
        /// </summary>
        VoiceStateUpdate = 4,

        /// <summary>
        /// Resume a previous session that was disconnected.
        /// </summary>
        Resume = 6,

        /// <summary>
        /// You should attempt to reconnect and resume immediately.
        /// </summary>
        Reconnect = 7,

        /// <summary>
        /// Request information about offline guild members in a large guild.
        /// </summary>
        RequestGuildMembers = 8,

        /// <summary>
        /// The session has been invalidated. You should reconnect and identify/resume accordingly.
        /// </summary>
        InvalidSession = 9,

        /// <summary>
        /// Sent immediately after connecting, contains the heartbeat_interval to use.
        /// </summary>
        Hello = 10,

        /// <summary>
        /// Sent in response to receiving a heartbeat to acknowledge that it has been received.
        /// </summary>
        HeartbeatACK = 11,
    }
}
