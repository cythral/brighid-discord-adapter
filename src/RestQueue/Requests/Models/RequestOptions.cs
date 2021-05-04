using System;

namespace Brighid.Discord.RestQueue.Requests
{
    /// <summary>
    /// Options used for requests.
    /// </summary>
    public class RequestOptions
    {
        /// <summary>
        /// Gets or sets the base URL used for requests.
        /// </summary>
        public Uri BaseUrl { get; set; } = new Uri("https://discord.com/api");
    }
}
