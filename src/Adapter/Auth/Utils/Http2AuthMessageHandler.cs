using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Adapter.Auth
{
    /// <summary>
    /// Handler for retrieving OpenID metadata over HTTP2.
    /// </summary>
    public class Http2AuthMessageHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Http2AuthMessageHandler"/> class.
        /// </summary>
        public Http2AuthMessageHandler()
        {
            InnerHandler = new HttpClientHandler();
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            request.Version = new Version(2, 0);
            request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
