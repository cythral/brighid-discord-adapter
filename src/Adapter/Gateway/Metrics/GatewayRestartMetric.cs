using Brighid.Discord.Adapter.Metrics;

namespace Brighid.Discord.Adapter.Gateway
{
    /// <summary>
    /// Hello Event Metric.
    /// </summary>
    public struct GatewayRestartMetric : IMetric<double>
    {
        /// <inheritdoc />
        public string Name => "GatewayRestart";

        /// <inheritdoc />
        public double Value => 1;
    }
}
