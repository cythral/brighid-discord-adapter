using Brighid.Discord.GatewayAdapter.Metrics;

namespace Brighid.Discord.GatewayAdapter.Events
{
    /// <summary>
    /// Reconnect Event Metric.
    /// </summary>
    public struct ResumedEventMetric : IMetric<double>
    {
        /// <inheritdoc />
        public string Name => "ResumedEvent";

        /// <inheritdoc />
        public double Value => 1;
    }
}
