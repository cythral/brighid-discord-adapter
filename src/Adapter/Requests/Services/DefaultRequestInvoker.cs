using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Requests
{
    /// <inheritdoc />
    public class DefaultRequestInvoker : IRequestInvoker
    {
        private readonly HttpMessageInvoker client;
        private readonly IUrlBuilder urlBuilder;
        private readonly IRequestMessageRelay relay;
        private readonly IBucketService bucketService;
        private readonly IBucketRepository bucketRepository;
        private readonly ILogger<DefaultRequestInvoker> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRequestInvoker" /> class.
        /// </summary>
        /// <param name="client">Client used to send/recieve http messages.</param>
        /// <param name="urlBuilder">Builder used to build URLs from requests.</param>
        /// <param name="relay">Service used to relay messages back and forth between SQS.</param>
        /// <param name="bucketService">Service used to manage buckets.</param>
        /// <param name="bucketRepository">Repository to manage buckets.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public DefaultRequestInvoker(
            HttpMessageInvoker client,
            IUrlBuilder urlBuilder,
            IRequestMessageRelay relay,
            IBucketService bucketService,
            IBucketRepository bucketRepository,
            ILogger<DefaultRequestInvoker> logger
        )
        {
            this.client = client;
            this.urlBuilder = urlBuilder;
            this.relay = relay;
            this.bucketService = bucketService;
            this.bucketRepository = bucketRepository;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Invoke(RequestMessage request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Bucket? bucket = null;

            try
            {
                bucket = await bucketService.GetBucketAndWaitForAvailability(request.RequestDetails, cancellationToken);
                var httpRequest = new HttpRequestMessage
                {
                    RequestUri = urlBuilder.BuildFromRequest(request.RequestDetails),
                    Method = request.RequestDetails.Endpoint.GetMethod(),
                    Content = CreateContent(request),
                };

                logger.LogDebug("Sending request uri:{@uri} method:{@method} body:{@body}", httpRequest.RequestUri, httpRequest.Method, httpRequest.Content);
                var httpResponse = await client.SendAsync(httpRequest, cancellationToken);
                var responseString = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                _ = relay.Respond(request, httpResponse.StatusCode, responseString, cancellationToken);
                _ = relay.Complete(request, httpResponse.StatusCode, responseString, cancellationToken);
                logger.LogDebug("Received response from {@uri}: {@response}", httpRequest.RequestUri, responseString);

                bucket = await bucketService.MergeBucketIds(bucket, httpResponse, cancellationToken);
                bucketService.UpdateBucketHitsRemaining(bucket, httpResponse);
                bucketService.UpdateBucketResetAfter(bucket, httpResponse);

                if (httpResponse.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    // throw exception here
                    logger.LogError(LogEvents.RestApiRateLimited, "Rest API call was rate limited.");
                }
            }
            catch (Exception exception)
            {
                logger.LogError(LogEvents.RestApiFailed, exception, "Rest API call failed.");
                _ = relay.Fail(request, 0, cancellationToken);
            }

            if (bucket != null)
            {
                await bucketRepository.Save(bucket, cancellationToken);
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
