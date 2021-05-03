using Brighid.Discord.GatewayAdapter.Metrics;

namespace Brighid.Discord.GatewayAdapter.Events
{
    /// <summary>
    /// Reconnect Event Metric.
    /// </summary>
    public struct InvalidSessionEventMetric : IMetric<double>
    {
        /// <inheritdoc />
        public string Name => "InvalidSessionEvent";

        /// <inheritdoc />
        public double Value => 1;
    }
}
