using Brighid.Discord.Adapter.Metrics;

namespace Brighid.Discord.Adapter.Events
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
