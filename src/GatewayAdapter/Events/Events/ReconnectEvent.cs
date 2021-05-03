namespace Brighid.Discord.GatewayAdapter.Events
{
    /// <summary>
    /// The reconnect event is dispatched when a client should reconnect to the gateway (and resume their existing session, if they have one). This event usually occurs during deploys to migrate sessions gracefully off old hosts.
    /// </summary>
    [GatewayEvent(GatewayOpCode.Reconnect)]
    public struct ReconnectEvent : IGatewayEvent
    {
    }
}
