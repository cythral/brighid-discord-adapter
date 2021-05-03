using Brighid.Discord.GatewayAdapter.Metrics;

namespace Brighid.Discord.GatewayAdapter.Events
{
    /// <summary>
    /// Reconnect Event Metric.
    /// </summary>
    public struct ReconnectEventMetric : IMetric<double>
    {
        /// <inheritdoc />
        public string Name => "ReconnectEvent";

        /// <inheritdoc />
        public double Value => 1;
    }
}
