namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Heartbeat Acknowledgement.
    /// </summary>
    [GatewayEvent(GatewayOpCode.HeartbeatACK)]
    public struct HeartbeatAckEvent : IGatewayEvent
    {
    }
}
