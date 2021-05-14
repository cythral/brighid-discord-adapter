using Brighid.Discord.Adapter.Metrics;

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Hello Event Metric.
    /// </summary>
    public struct RestApiRateLimitedMetric : IMetric<double>
    {
        /// <inheritdoc />
        public string Name => "RestApiRateLimited";

        /// <inheritdoc />
        public double Value => 1;
    }
}
