using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;
using Brighid.Discord.Serialization;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Requests
{
    /// <inheritdoc />
    public class DefaultRequestMessageRelayHttpClient : IRequestMessageRelayHttpClient
    {
        private readonly System.Net.Http.HttpClient httpClient;
        private readonly ILogger<DefaultRequestMessageRelayHttpClient> logger;
        private readonly ISerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRequestMessageRelayHttpClient" /> class.
        /// </summary>
        /// <param name="httpClient">Inner HTTP Client to use.</param>
        /// <param name="serializer">Service for deserializing/serializing messages.</param>
        /// <param name="logger">Service used for logging info to the console.</param>
        public DefaultRequestMessageRelayHttpClient(
            System.Net.Http.HttpClient httpClient,
            ISerializer serializer,
            ILogger<DefaultRequestMessageRelayHttpClient> logger
        )
        {
            this.httpClient = httpClient;
            this.serializer = serializer;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Post(Uri url, Response response, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var @bytes = serializer.SerializeToBytes(response);
            var content = new ByteArrayContent(@bytes);
            var postResponse = await httpClient.PostAsync(url, content, cancellationToken);

            if (!postResponse.IsSuccessStatusCode)
            {
                logger.LogError("An error occurred while responding to a discord API request.  Client returned {@statusCode} status code.", postResponse.StatusCode);
            }
        }
    }
}
