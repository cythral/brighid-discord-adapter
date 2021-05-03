using Brighid.Discord.GatewayAdapter.Metrics;

namespace Brighid.Discord.GatewayAdapter.Events
{
    /// <summary>
    /// Hello Event Metric.
    /// </summary>
    public struct HelloEventMetric : IMetric<double>
    {
        /// <inheritdoc />
        public string Name => "HelloEvent";

        /// <inheritdoc />
        public double Value => 1;
    }
}
