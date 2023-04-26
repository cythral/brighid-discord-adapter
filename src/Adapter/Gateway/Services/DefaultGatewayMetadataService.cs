using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Brighid.Discord.Adapter.Management;
using Brighid.Discord.RestClient.Client;

namespace Brighid.Discord.Adapter.Gateway
{
    /// <inheritdoc />
    public class DefaultGatewayMetadataService : IGatewayMetadataService
    {
        private readonly IDiscordGatewayClient gatewayClient;
        private readonly IDiscordApiInfoService apiInfoService;
        private Uri? gatewayUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayMetadataService" /> class.
        /// </summary>
        /// <param name="gatewayClient">Discord Gateway Client API service.</param>
        /// <param name="apiInfoService">Service for getting info about the discord API.</param>
        public DefaultGatewayMetadataService(
            IDiscordGatewayClient gatewayClient,
            IDiscordApiInfoService apiInfoService
        )
        {
            this.gatewayClient = gatewayClient;
            this.apiInfoService = apiInfoService;
        }

        /// <inheritdoc />
        public ValueTask<Uri> GetGatewayUrl(CancellationToken cancellationToken)
        {
            return gatewayUrl != null ? new ValueTask<Uri>(gatewayUrl) : GetGatewayUrlAsync(cancellationToken);
        }

        /// <inheritdoc />
        public void SetGatewayUrl(Uri? gatewayUrl)
        {
            if (gatewayUrl == null)
            {
                this.gatewayUrl = null;
                return;
            }

            var queryString = BuildQueryString();
            this.gatewayUrl = new UriBuilder(gatewayUrl!) { Query = queryString }.Uri;
        }

        private async ValueTask<Uri> GetGatewayUrlAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var gatewayInfo = await gatewayClient.GetGatewayBotInfo(cancellationToken);
            var queryString = BuildQueryString();

            gatewayUrl = new UriBuilder(gatewayInfo.Url) { Query = queryString }.Uri;
            return gatewayUrl;
        }

        private string? BuildQueryString()
        {
            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["v"] = apiInfoService.ApiVersion;
            queryParams["encoding"] = "json";

            return queryParams.ToString();
        }
    }
}
