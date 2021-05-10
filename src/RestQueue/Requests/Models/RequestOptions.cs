using System;

namespace Brighid.Discord.RestQueue.Requests
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
        /// Gets or sets the number of seconds to buffer delete message requests for.
        /// </summary>
        public double MessageBufferPeriod { get; set; } = 30;

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
