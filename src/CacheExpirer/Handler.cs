using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Lambdajection.Attributes;
using Lambdajection.Sns;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Brighid.Discord.CacheExpirer
{
    [SnsEventHandler(typeof(Startup))]
    public partial class Handler
    {
        private delegate Task ExpireFunction(IPAddress address, string id, CancellationToken cancellationToken);

        private readonly IDnsService dns;
        private readonly ICacheService cache;
        private readonly ILogger<Handler> logger;
        private readonly CacheExpirerOptions options;

        public Handler(
            IDnsService dns,
            ICacheService cache,
            IOptions<CacheExpirerOptions> options,
            ILogger<Handler> logger
        )
        {
            this.dns = dns;
            this.cache = cache;
            this.logger = logger;
            this.options = options.Value;
        }

        public async Task<string> Handle(SnsMessage<CacheExpirationRequest> request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ExpireFunction expireFunction = request.Message.Type switch
            {
                CacheExpirationType.User => cache.ExpireUserCache,
                CacheExpirationType.Command => cache.ExpireCommandCache,
                _ => throw new NotSupportedException(),
            };

            var ipAddresses = await dns.GetIPAddresses(options.AdapterUrl, cancellationToken);
            logger.LogInformation("Found IP Addresses: {@ipAddresses}", string.Join(',', ipAddresses));
            var tasks = ipAddresses.Select(ip => expireFunction(ip, request.Message.Id, cancellationToken));
            await Task.WhenAll(tasks);

            return string.Empty;
        }
    }
}
