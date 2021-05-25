using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;
using Brighid.Discord.Serialization;
using Brighid.Discord.Threading;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.RestClient.Responses
{
    /// <inheritdoc />
    public class DefaultResponseServer : IResponseServer
    {
        private readonly ITcpListener listener;
        private readonly ISerializer serializer;
        private readonly ITimerFactory timerFactory;
        private readonly IRequestMap requestMap;
        private readonly ILogger<DefaultResponseServer> logger;
        private ITimer? timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultResponseServer" /> class.
        /// </summary>
        /// <param name="listener">Listener to accept TCP client connections from.</param>
        /// <param name="serializer">Serializer to serialize things between formats.</param>
        /// <param name="timerFactory">Factory used to create the timer.</param>
        /// <param name="requestMap">Map of requests to listen for responses.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public DefaultResponseServer(
            ITcpListener listener,
            ISerializer serializer,
            ITimerFactory timerFactory,
            IRequestMap requestMap,
            ILogger<DefaultResponseServer> logger
        )
        {
            this.listener = listener;
            this.serializer = serializer;
            this.timerFactory = timerFactory;
            this.requestMap = requestMap;
            this.logger = logger;
        }

        /// <inheritdoc />
        public bool IsRunning { get; set; }

        /// <inheritdoc />
        public Uri Uri
        {
            get
            {
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    var ips =
                        from address in nic.GetIPProperties().UnicastAddresses
                        where address.Address.AddressFamily == AddressFamily.InterNetwork &&
                            !IPAddress.IsLoopback(address.Address)
                        select address.Address;

                    if (ips.Any())
                    {
                        var ip = ips.First();
                        return new Uri($"http://{ip}:{listener.Port}");
                    }
                }

                throw new InvalidOperationException("Could not find an accessible IP Address.");
            }
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            timer = timerFactory.CreateTimer(Run, 0, "Response Server");

            listener.Start();
            await timer.Start();
            IsRunning = true;
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfNotRunning(nameof(StopAsync));

            IsRunning = false;
            await timer!.Stop();
            listener.Stop();

            timer = null;
        }

        /// <summary>
        /// Handles an incoming TCP Client.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task Run(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var client = await listener.Accept();
            var stream = client.GetStream();
            var response = await serializer.Deserialize<Response>(stream, cancellationToken);
            logger.LogInformation("Received response: {@response}", response);

            if (requestMap.TryGetValue(response.RequestId, out var promise))
            {
                promise.TrySetResult(response);
                requestMap.Remove(response.RequestId);
            }
        }

        /// <inheritdoc />
        public Task<Response> ListenForResponse(Guid requestId, TaskCompletionSource<Response> promise)
        {
            requestMap.Add(requestId, promise);
            return promise.Task;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfNotRunning(string name)
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException($"Cannot call {name} while the server is not running.");
            }
        }
    }
}
