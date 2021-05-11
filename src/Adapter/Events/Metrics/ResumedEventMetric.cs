using Brighid.Discord.Adapter.Metrics;

namespace Brighid.Discord.Adapter.Events
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
