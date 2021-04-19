using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.Extensions.Options;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Metrics
{
    public class CloudWatchMetricReporterTests
    {
        [TestFixture]
        public class ReportTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                IMetric<double> metric,
                [Frozen, Substitute] IAmazonCloudWatch cloudwatch,
                [Target] CloudWatchMetricReporter reporter
            )
            {
                var cancellationToken = new CancellationToken(true);
                Func<Task> func = () => reporter.Report(metric, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
                await cloudwatch.DidNotReceive().PutMetricDataAsync(Any<PutMetricDataRequest>(), Any<CancellationToken>());
            }

            [Test, Auto]
            public async Task ShouldReportMetricDataToCloudWatchWithMetricName(
                IMetric<double> metric,
                [Frozen, Substitute] IAmazonCloudWatch cloudwatch,
                [Target] CloudWatchMetricReporter reporter
            )
            {
                var cancellationToken = new CancellationToken(false);

                await reporter.Report(metric, cancellationToken);

                await cloudwatch.Received().PutMetricDataAsync(Any<PutMetricDataRequest>(), Any<CancellationToken>());
                var request = (PutMetricDataRequest)cloudwatch.ReceivedCalls().ElementAt(0).GetArguments()[0];

                request.MetricData.Should().Contain(data =>
                    data.MetricName == metric.Name
                );
            }

            [Test, Auto]
            public async Task ShouldReportMetricDataToCloudWatchWithValue(
                IMetric<double> metric,
                [Frozen, Substitute] IAmazonCloudWatch cloudwatch,
                [Target] CloudWatchMetricReporter reporter
            )
            {
                var cancellationToken = new CancellationToken(false);

                await reporter.Report(metric, cancellationToken);

                await cloudwatch.Received().PutMetricDataAsync(Any<PutMetricDataRequest>(), Any<CancellationToken>());
                var request = (PutMetricDataRequest)cloudwatch.ReceivedCalls().ElementAt(0).GetArguments()[0];

                request.MetricData.Should().Contain(data =>
                    data.Value == metric.Value
                );
            }

            [Test, Auto]
            public async Task ShouldReportMetricDataToCloudWatchWithNamespace(
                string metricNamespace,
                IMetric<double> metric,
                [Frozen, Options] IOptions<MetricOptions> options,
                [Frozen, Substitute] IAmazonCloudWatch cloudwatch,
                [Target] CloudWatchMetricReporter reporter
            )
            {
                var cancellationToken = new CancellationToken(false);
                options.Value.Namespace = metricNamespace;

                await reporter.Report(metric, cancellationToken);

                await cloudwatch.Received().PutMetricDataAsync(Any<PutMetricDataRequest>(), Any<CancellationToken>());
                var request = (PutMetricDataRequest)cloudwatch.ReceivedCalls().ElementAt(0).GetArguments()[0];
                request.Namespace.Should().Be(metricNamespace);
            }
        }
    }
}
