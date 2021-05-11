using System;

namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// Options used for requests.
    /// </summary>
    public class RequestOptions
    {
        /// <summary>
        /// Gets or sets the queue url to retrieve requests from.
        /// </summary>
        public Uri QueueUrl { get; set; } = new Uri("http://localhost");

        /// <summary>
        /// Gets or sets the number of milliseconds to wait between polls.
        /// </summary>
        public int PollingInterval { get; set; } = 2000;

        /// <summary>
        /// Gets or sets the max number of seconds to wait for messages when requesting from SQS.
        /// </summary>
        public int MessageWaitTime { get; set; } = 20;

        /// <summary>
        /// Gets or sets the number of seconds to buffer delete message requests for.
        /// </summary>
        public double BatchingBufferPeriod { get; set; } = 30;

        /// <summary>
        /// Gets or sets the maximum amount of messages than can be received from SQS at once.
        /// </summary>
        public uint MessageBufferSize { get; set; } = 10;

        /// <summary>
        /// Gets or sets the base URL used for invoking requests.
        /// </summary>
        public Uri InvokeBaseUrl { get; set; } = new Uri("https://discord.com/api");
    }
}
