using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.RestClient.Responses
{
    /// <inheritdoc />
    public class DefaultResponseService : IResponseService
    {
        private readonly IRequestMap requestMap;
        private Uri? uri;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultResponseService" /> class.
        /// </summary>
        /// <param name="requestMap">Map that keeps track of requests waiting for a response.</param>
        public DefaultResponseService(
            IRequestMap requestMap
        )
        {
            this.requestMap = requestMap;
        }

        /// <inheritdoc />
        public Uri Uri
        {
            get
            {
                if (uri == null)
                {
                    foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        var ips =
                            from address in networkInterface.GetIPProperties().UnicastAddresses
                            where address.Address.AddressFamily == AddressFamily.InterNetworkV6 &&
                                !IPAddress.IsLoopback(address.Address) && !address.Address.IsIPv6LinkLocal
                            select address.Address;

                        if (ips.Any())
                        {
                            var ip = ips.First();
                            uri = new Uri($"http://[{ip}]/brighid/discord/rest-response");
                            break;
                        }
                    }
                }

                return uri ?? throw new Exception("Could not locate server address.");
            }
        }

        /// <inheritdoc />
        public void HandleResponse(Response response)
        {
            if (requestMap.TryGetValue(response.RequestId, out var promise))
            {
                promise.SetResult(response);
                requestMap.Remove(response.RequestId);
            }
        }

        /// <inheritdoc />
        public async Task<Response> ListenForResponse(Guid requestId, TaskCompletionSource<Response> promise, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            requestMap.Add(requestId, promise);

            try
            {
                return await promise.Task.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException exception)
            {
                requestMap.Remove(requestId);
                throw exception;
            }
        }
    }
}
