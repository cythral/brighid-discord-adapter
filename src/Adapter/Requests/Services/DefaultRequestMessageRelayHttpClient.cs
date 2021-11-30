using System;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.Requests
{
    /// <inheritdoc />
    public class DefaultRequestMessageRelayHttpClient : IRequestMessageRelayHttpClient
    {
        private readonly System.Net.Http.HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRequestMessageRelayHttpClient" /> class.
        /// </summary>
        /// <param name="httpClient">Inner HTTP Client to use.</param>
        public DefaultRequestMessageRelayHttpClient(System.Net.Http.HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <inheritdoc />
        public async Task Post(Uri url, Response response, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await httpClient.PostAsJsonAsync(url, response, cancellationToken);
        }
    }
}
