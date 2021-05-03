using Brighid.Discord.GatewayAdapter.Metrics;

namespace Brighid.Discord.GatewayAdapter.Gateway
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
