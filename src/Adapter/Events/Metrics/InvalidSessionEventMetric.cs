using Brighid.Discord.Adapter.Metrics;

namespace Brighid.Discord.Adapter.Events
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
