using Brighid.Discord.Metrics;

namespace Brighid.Discord.Events
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
