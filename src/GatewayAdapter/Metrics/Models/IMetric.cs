namespace Brighid.Discord.GatewayAdapter.Metrics
{
    /// <summary>
    /// Represents an Application Metric.
    /// </summary>
    /// <typeparam name="TValue">The type of value the metric is.</typeparam>
    public interface IMetric<TValue>
    {
        /// <summary>
        /// Gets the name of the metric.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the value of the metric.
        /// </summary>
        TValue Value { get; }
    }
}
