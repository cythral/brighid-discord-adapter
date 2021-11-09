using System.Diagnostics.CodeAnalysis;

namespace Brighid.Discord.Adapter.Metrics
{
    /// <summary>
    /// Options to use when publishing metrics.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public class MetricOptions
    {
        /// <summary>
        /// Gets or sets the namespace to put metrics in.
        /// </summary>
        public string Namespace { get; set; } = string.Empty;
    }
}
