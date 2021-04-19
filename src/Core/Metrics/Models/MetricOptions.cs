namespace Brighid.Discord.Metrics
{
    /// <summary>
    /// Options to use when publishing metrics.
    /// </summary>
    public class MetricOptions
    {
        /// <summary>
        /// Gets or sets the namespace to put metrics in.
        /// </summary>
        public string Namespace { get; set; } = string.Empty;
    }
}
