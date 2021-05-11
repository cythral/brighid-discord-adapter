using Brighid.Discord.Adapter.Metrics;

namespace Brighid.Discord.Adapter.Events
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
