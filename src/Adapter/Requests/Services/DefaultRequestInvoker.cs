using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Requests
{
    /// <inheritdoc />
    public class DefaultRequestInvoker : IRequestInvoker
    {
        private readonly HttpMessageInvoker client;
        private readonly IUrlBuilder urlBuilder;
        private readonly IRequestMessageRelay relay;
        private readonly ILogger<DefaultRequestInvoker> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRequestInvoker" /> class.
        /// </summary>
        /// <param name="client">Client used to send/recieve http messages.</param>
        /// <param name="urlBuilder">Builder used to build URLs from requests.</param>
        /// <param name="relay">Service used to relay messages back and forth between SQS.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public DefaultRequestInvoker(
            HttpMessageInvoker client,
            IUrlBuilder urlBuilder,
            IRequestMessageRelay relay,
            ILogger<DefaultRequestInvoker> logger
        )
        {
            this.client = client;
            this.urlBuilder = urlBuilder;
            this.relay = relay;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Invoke(RequestMessage request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var httpRequest = new HttpRequestMessage
                {
                    RequestUri = urlBuilder.BuildFromRequest(request.RequestDetails),
                    Method = request.RequestDetails.Endpoint.GetMethod(),
                    Content = CreateContent(request),
                };

                logger.LogInformation("Sending request uri:{@uri} method:{@method} body:{@body}", httpRequest.RequestUri, httpRequest.Method, httpRequest.Content);
                var httpResponse = await client.SendAsync(httpRequest, cancellationToken);
                var responseString = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                logger.LogInformation("Received response from {@uri}: {@response}", httpRequest.RequestUri, responseString);

                await relay.Complete(request, httpResponse.StatusCode, responseString, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError("Error occurred while attempting to invoke request: {@exception}", exception);
                await relay.Fail(request, 0, cancellationToken);
            }
        }

        private static HttpContent? CreateContent(RequestMessage request)
        {
            if (request.RequestDetails.RequestBody == null)
            {
                return null;
            }

            var content = new StringContent(request.RequestDetails.RequestBody);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
        }
    }
}
