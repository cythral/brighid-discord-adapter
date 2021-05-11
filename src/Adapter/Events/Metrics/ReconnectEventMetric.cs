using Brighid.Discord.Adapter.Metrics;

namespace Brighid.Discord.Adapter.Events
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
