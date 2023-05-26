using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            try
            {
                logger.LogDebug("Sending response to {@url} for discord API request {@requestId}", url, response.RequestId);
                var postResponse = await httpClient.PostAsync(url, content, cancellationToken);
                logger.LogDebug("Responded to a discord API request {@requestId} with status code {@statusCode}", response.RequestId, response.StatusCode);
                postResponse.EnsureSuccessStatusCode();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred while responding to a discord API request.");
            }
        }
    }
}
