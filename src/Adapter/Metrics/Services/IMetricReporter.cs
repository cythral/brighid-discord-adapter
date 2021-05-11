using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Adapter.Metrics
{
    /// <summary>
    /// Service used to report metrics.
    /// </summary>
    public interface IMetricReporter
    {
        /// <summary>
        /// Reports an application metric.
        /// </summary>
        /// <param name="metric">The metric to report.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Report(IMetric<double> metric, CancellationToken cancellationToken = default);
    }
}
