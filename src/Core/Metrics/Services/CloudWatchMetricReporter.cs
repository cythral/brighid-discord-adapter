using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Brighid.Discord.Metrics
{
    /// <summary>
    /// Metric reporting services that reports metrics to CloudWatch.
    /// </summary>
    [LogCategory("Metric Reporter")]
    public class CloudWatchMetricReporter : IMetricReporter
    {
        private readonly IAmazonCloudWatch cloudwatch;
        private readonly MetricOptions options;
        private readonly ILogger<CloudWatchMetricReporter> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudWatchMetricReporter" /> class.
        /// </summary>
        /// <param name="cloudwatch">CloudWatch Client to use for reporting metrics.</param>
        /// <param name="options">Options to use when reporting metrics.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public CloudWatchMetricReporter(
            IAmazonCloudWatch cloudwatch,
            IOptions<MetricOptions> options,
            ILogger<CloudWatchMetricReporter> logger
        )
        {
            this.cloudwatch = cloudwatch;
            this.options = options.Value;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Report(IMetric<double> metric, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Putting Metric to CloudWatch. Name: {@name} Value: {@value}", metric.Name, metric.Value);

            using var scope = logger.BeginScope("{@ApiCall}", "cloudwatch:PutMetricData");

            var request = CreateRequest(metric);
            logger.LogInformation("Sending request: {@request}", request);

            var response = await cloudwatch.PutMetricDataAsync(request, cancellationToken);
            logger.LogInformation("Received response: {@response}", response);
        }

        private PutMetricDataRequest CreateRequest(IMetric<double> metric)
        {
            return new PutMetricDataRequest
            {
                Namespace = options.Namespace,
                MetricData = new List<MetricDatum>
                {
                    new() { MetricName = metric.Name, Value = metric.Value },
                },
            };
        }
    }
}
