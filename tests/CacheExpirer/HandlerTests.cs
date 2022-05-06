using System.Net;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.NUnit3;

using Lambdajection.Sns;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.CacheExpirer
{
    public class HandlerTests
    {
        [TestFixture]
        [Category("Unit")]
        public class HandleTests
        {
            [Test, Auto]
            public async Task ShouldClearUserCache(
                string userId,
                IPAddress ip1,
                IPAddress ip2,
                SnsMessage<CacheExpirationRequest> request,
                [Frozen] IDnsService dns,
                [Frozen] ICacheService cache,
                [Frozen] CacheExpirerOptions options,
                [Target] Handler handler,
                CancellationToken cancellationToken
            )
            {
                request.Message.Type = CacheExpirationType.User;
                request.Message.Id = userId;
                dns.GetIPAddresses(Any<string>(), Any<CancellationToken>()).Returns(new[] { ip1, ip2 });

                await handler.Handle(request, cancellationToken);

                await dns.Received().GetIPAddresses(Is(options.AdapterUrl), Is(cancellationToken));

                await cache.Received().ExpireUserCache(Is(ip1), Is(userId), Is(cancellationToken));
                await cache.Received().ExpireUserCache(Is(ip2), Is(userId), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldClearCommandCache(
                string command,
                IPAddress ip1,
                IPAddress ip2,
                SnsMessage<CacheExpirationRequest> request,
                [Frozen] IDnsService dns,
                [Frozen] ICacheService cache,
                [Frozen] CacheExpirerOptions options,
                [Target] Handler handler,
                CancellationToken cancellationToken
            )
            {
                request.Message.Type = CacheExpirationType.Command;
                request.Message.Id = command;
                dns.GetIPAddresses(Any<string>(), Any<CancellationToken>()).Returns(new[] { ip1, ip2 });

                await handler.Handle(request, cancellationToken);

                await dns.Received().GetIPAddresses(Is(options.AdapterUrl), Is(cancellationToken));

                await cache.Received().ExpireCommandCache(Is(ip1), Is(command), Is(cancellationToken));
                await cache.Received().ExpireCommandCache(Is(ip2), Is(command), Is(cancellationToken));
            }
        }
    }
}
