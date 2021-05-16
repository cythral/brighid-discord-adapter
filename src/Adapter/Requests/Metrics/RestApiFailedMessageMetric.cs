using Brighid.Discord.Adapter.Metrics;

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Hello Event Metric.
    /// </summary>
    public struct RestApiFailedMessageMetric : IMetric<double>
    {
        /// <inheritdoc />
        public string Name => "RestApiFailedMessage";

        /// <inheritdoc />
        public double Value => 1;
    }
}
