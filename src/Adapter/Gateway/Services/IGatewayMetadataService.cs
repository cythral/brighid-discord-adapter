using System;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Adapter.Gateway
{
    /// <summary>
    /// Service for dealing with Gateway URLs.
    /// </summary>
    public interface IGatewayMetadataService
    {
        /// <summary>
        /// Gets the URL of the gateway to connect to.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The gateway url to connect to.</returns>
        ValueTask<Uri> GetGatewayUrl(CancellationToken cancellationToken);

        /// <summary>
        /// Sets the gateway URL to connect to.
        /// </summary>
        /// <param name="gatewayUrl">URL of the gateway to connect to.</param>
        void SetGatewayUrl(Uri? gatewayUrl);
    }
}
