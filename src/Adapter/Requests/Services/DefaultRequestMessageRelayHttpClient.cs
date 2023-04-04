using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;
using Brighid.Discord.Serialization;

namespace Brighid.Discord.Adapter.Requests
{
    /// <inheritdoc />
    public class DefaultRequestMessageRelayHttpClient : IRequestMessageRelayHttpClient
    {
        private readonly System.Net.Http.HttpClient httpClient;
        private readonly ISerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRequestMessageRelayHttpClient" /> class.
        /// </summary>
        /// <param name="httpClient">Inner HTTP Client to use.</param>
        /// <param name="serializer">Service for deserializing/serializing messages.</param>
        public DefaultRequestMessageRelayHttpClient(
            System.Net.Http.HttpClient httpClient,
            ISerializer serializer
        )
        {
            this.httpClient = httpClient;
            this.serializer = serializer;
        }

        /// <inheritdoc />
        public async Task Post(Uri url, Response response, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var @bytes = serializer.SerializeToBytes(response);
            var content = new ByteArrayContent(@bytes);
            await httpClient.PostAsync(url, content, cancellationToken);
        }
    }
}
