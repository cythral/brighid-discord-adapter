using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.NUnit3;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using RichardSzalay.MockHttp;

namespace Brighid.Discord.Adapter.Management
{
    public class TrafficShifterTests
    {
        [TestFixture]
        [Category("Unit")]
        public class PerformTrafficShiftTests
        {
            public static void SetupNode(NodeInfo node, int shard, string ip, MockHttpMessageHandler http, bool expected)
            {
                node.Shard = shard;
                node.IpAddress = IPAddress.Parse(ip);

                if (expected)
                {
                    http
                    .Expect(HttpMethod.Put, $"http://{ip}/node/gateway/state")
                    .Respond(HttpStatusCode.NoContent);
                }
            }

            [Test, Auto]
            public async Task ShouldStopGatewayForPeersWithTheSameShard(
                NodeInfo currentNode,
                NodeInfo peer1,
                NodeInfo peer2,
                NodeInfo peer3,
                [Frozen] MockHttpMessageHandler http,
                [Frozen] IAdapterContext context,
                [Target] TrafficShifter shifter,
                CancellationToken cancellationToken
            )
            {
                currentNode.Shard = 2;
                SetupNode(peer1, 2, "1.1.1.1", http, true);
                SetupNode(peer2, 1, "2.2.2.2", http, false);
                SetupNode(peer3, 2, "3.3.3.3", http, true);

                context.Get<NodeInfo>().Returns(currentNode);
                context.Get<IEnumerable<NodeInfo>>().Returns(new[] { peer1, peer2, peer3 });

                await shifter.PerformTrafficShift(cancellationToken);

                http.VerifyNoOutstandingExpectation();
            }

            [Test, Auto]
            public async Task ShouldHandleErrorsGracefully(
                NodeInfo currentNode,
                NodeInfo peer1,
                NodeInfo peer2,
                NodeInfo peer3,
                [Frozen] MockHttpMessageHandler http,
                [Frozen] IAdapterContext context,
                [Target] TrafficShifter shifter,
                CancellationToken cancellationToken
            )
            {
                currentNode.Shard = 2;
                SetupNode(peer1, 2, "1.1.1.1", http, true);
                SetupNode(peer2, 1, "2.2.2.2", http, false);
                SetupNode(peer3, 2, "3.3.3.3", http, false);

                context.Get<NodeInfo>().Returns(currentNode);
                context.Get<IEnumerable<NodeInfo>>().Returns(new[] { peer1, peer2, peer3 });

                Func<Task> func = () => shifter.PerformTrafficShift(cancellationToken);

                await func.Should().NotThrowAsync();
            }
        }
    }
}
