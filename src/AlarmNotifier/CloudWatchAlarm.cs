using System;

namespace Brighid.Discord.AlarmNotifier
{
    public class CloudWatchAlarm
    {
        // {\"AlarmName\":\"Example alarm name\",\"AlarmDescription\":\"Example alarm description.\",\"AWSAccountId\":\"000000000000\",\"NewStateValue\":\"ALARM\",\"NewStateReason\":\"Threshold Crossed: 1 datapoint (10.0) was greater than or equal to the threshold (1.0).\",\"StateChangeTime\":\"2017-01-12T16:30:42.236+0000\",\"Region\":\"EU - Ireland\",\"OldStateValue\":\"OK\",\"Trigger\":{\"MetricName\":\"DeliveryErrors\",\"Namespace\":\"ExampleNamespace\",\"Statistic\":\"SUM\",\"Unit\":null,\"Dimensions\":[],\"Period\":300,\"EvaluationPeriods\":1,\"ComparisonOperator\":\"GreaterThanOrEqualToThreshold\",\"Threshold\":1.0}}

        /// <summary>
        /// Gets or sets the name of the alarm being triggered.
        /// </summary>
        public string AlarmName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the alarm being triggered.
        /// </summary>
        public string AlarmDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the new state value of the alarm.
        /// </summary>
        public string NewStateValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the reason for the new state of the alarm.
        /// </summary>
        public string NewStateReason { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the time when the state of the alarm changed.
        /// </summary>
        public string StateChangeTime { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ARN of the alarm.
        /// </summary>
        public string AlarmArn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the old state of the alarm.
        /// </summary>
        public string OldStateValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of actions that are triggered by this alarm returning to a normal state.
        /// </summary>
        public string[] OKActions { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the list of actions that are triggered by this alarm going into an alarm state.
        /// </summary>
        public string[] AlarmActions { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the list of actions that are triggered by this alarm going into an insufficient data state.
        /// </summary>
        public string[] InsufficientDataActions { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the metric that triggered this alarm.
        /// </summary>
        public Metric Trigger { get; set; } = new();

        public class Metric
        {
            /// <summary>
            /// Gets or sets the name of the metric.
            /// </summary>
            public string MetricName { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the name of the namespace.
            /// </summary>
            public string Namespace { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the type of statistic this metric uses.
            /// </summary>
            public string StatisticType { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the statistic this metric uses.
            /// </summary>
            public string Statistic { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the unit this metric uses.
            /// </summary>
            public string Unit { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the period of this metric.
            /// </summary>
            public int Period { get; set; }

            /// <summary>
            /// Gets or sets the number of periods evaluated for this metric.
            /// </summary>
            public int EvaluationPeriods { get; set; }

            /// <summary>
            /// Gets or sets the comparison operator used to evaluate the metric.
            /// </summary>
            public string ComparisonOperator { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the threshold of the alarm being breached.
            /// </summary>
            public double Threshold { get; set; }

            /// <summary>
            /// Gets or sets a value indicating how to treat missing data on this metric.
            /// </summary>
            public string TreatMissingData { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the evaluate low sample count percentile for this metric.
            /// </summary>
            public string EvaluateLowSampleCountPercentile { get; set; } = string.Empty;
        }
    }
}
