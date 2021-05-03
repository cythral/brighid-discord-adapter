using Brighid.Discord.GatewayAdapter.Metrics;

namespace Brighid.Discord.GatewayAdapter.Events
{
    /// <summary>
    /// Hello Event Metric.
    /// </summary>
    public struct MessageCreateEventMetric : IMetric<double>
    {
        /// <inheritdoc />
        public string Name => "MessageCreateEvent";

        /// <inheritdoc />
        public double Value => 1;
    }
}
