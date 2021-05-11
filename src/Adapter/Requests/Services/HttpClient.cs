using System.Net.Http.Headers;

using Microsoft.Extensions.Options;

namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// Client used to make HTTP requests.
    /// </summary>
    public class HttpClient : System.Net.Http.HttpClient
    {
        private readonly AdapterOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClient" /> class.
        /// </summary>
        /// <param name="options">Options to use for making HTTP requests.</param>
        public HttpClient(
            IOptions<AdapterOptions> options
        )
        {
            this.options = options.Value;
            Configure();
        }

        private void Configure()
        {
            DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(options.AuthScheme, options.Token);
        }
    }
}
