namespace Brighid.Discord.Events
{
    /// <summary>
    /// Used to replay missed events when a disconnected client resumes.
    /// </summary>
    [GatewayEvent(GatewayOpCode.Dispatch, "RESUMED")]
    public struct ResumedEvent : IGatewayEvent
    {
    }
}
