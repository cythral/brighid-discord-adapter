namespace Brighid.Discord.RestClient.Client
{
    /// <summary>
    /// Options to use when making client requests.
    /// </summary>
    public class ClientOptions
    {
        /// <summary>
        /// Gets or sets the URL to queue requests to.
        /// </summary>
        public string RequestQueueUrl { get; set; } = string.Empty;
    }
}
