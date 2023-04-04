using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;

using Amazon.ECS;
using Amazon.ECS.Model;

using AutoFixture.NUnit3;

using Brighid.Discord.Serialization;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using RichardSzalay.MockHttp;

using static NSubstitute.Arg;

using JsonSerializer = System.Text.Json.JsonSerializer;
using Task = System.Threading.Tasks.Task;

namespace Brighid.Discord.Adapter.Management
{
    public class NodeServiceTests
    {
        [Category("Unit")]
        public class GetIpAddressTests
        {
            [Test, Auto]
            public async Task ShouldFetchIpAddress(
                string taskArn,
                string deploymentId,
                string cluster,
                IPAddress ip,
                [Frozen] TaskMetadata taskMetadata,
                [Frozen] AdapterOptions options,
                [Frozen] MockHttpMessageHandler handler,
                [Frozen] ISerializer serializer,
                [Target] NodeService service,
                CancellationToken cancellationToken
            )
            {
                taskMetadata.Containers[0].Networks[0].IPv6Addresses = new[] { ip.ToString() };
                handler
                .Expect(HttpMethod.Get, options.TaskMetadataUrl!.ToString())
                .Respond("application/json", $@"{{ ""Containers"": [ {{ ""Networks"": [ {{ ""IPv6Addresses"": [ ""{ip}"" ] }} ] }} ] }}");

                var result = await service.GetIpAddress(cancellationToken);

                result.Should().Be(ip);
                handler.VerifyNoOutstandingExpectation();
            }

            [Test, Auto]
            public async Task ShouldReturnNoneIfTheMetadataEndpointIsNull(
                [Frozen] AdapterOptions options,
                [Frozen] MockHttpMessageHandler handler,
                [Target] NodeService service,
                CancellationToken cancellationToken
            )
            {
                options.TaskMetadataUrl = null;

                var result = await service.GetIpAddress(cancellationToken);

                result.Should().Be(IPAddress.None);
            }
        }

        [Category("Unit")]
        public class GetDeploymentIdTests
        {
            [Test, Auto]
            public async Task ShouldFetchNodeDeploymentId(
                string taskArn,
                string deploymentId,
                string cluster,
                [Frozen] AdapterOptions options,
                [Frozen] DescribeTasksResponse describeResponse,
                [Frozen] TaskMetadata taskMetadata,
                [Frozen] IAmazonECS ecs,
                [Frozen] MockHttpMessageHandler handler,
                [Target] NodeService service,
                CancellationToken cancellationToken
            )
            {
                taskMetadata.TaskArn = taskArn;
                taskMetadata.Cluster = cluster;

                handler
                .Expect(HttpMethod.Get, options.TaskMetadataUrl!.ToString())
                .Respond("application/json", $@"{{ ""Cluster"": ""{cluster}"", ""TaskARN"": ""{taskArn}"" }}");

                describeResponse.Tasks[0].StartedBy = deploymentId;

                var result = await service.GetDeploymentId(cancellationToken);

                result.Should().Be(deploymentId);
                await ecs.Received().DescribeTasksAsync(Is<DescribeTasksRequest>(req => req.Cluster == cluster && req.Tasks[0] == taskArn), Is(cancellationToken));
                handler.VerifyNoOutstandingExpectation();
            }

            [Test, Auto]
            public async Task ShouldReturnLocalIfTheMetadataEndpointIsNull(
                [Frozen] AdapterOptions options,
                [Frozen] MockHttpMessageHandler handler,
                [Target] NodeService service,
                CancellationToken cancellationToken
            )
            {
                options.TaskMetadataUrl = null;

                var result = await service.GetDeploymentId(cancellationToken);

                result.Should().Be("local");
            }
        }

        [Category("Unit")]
        public class GetPeersTests
        {
            [Test, Auto]
            public async Task ShouldReturnAListOfPeers(
                [Frozen] IDnsService dnsService,
                [Frozen] MockHttpMessageHandler handler,
                [Frozen] ISerializer serializer,
                [Target] NodeService service,
                CancellationToken cancellationToken
            )
            {
                var node1Ip = IPAddress.Parse("ea9b:3c94:1791:fafb:f5fe:5c7e:05fc:af52");
                var node2Ip = IPAddress.Parse("8526:e466:415c:e5f2:4998:d840:bb1c:1e49");
                var node3Ip = IPAddress.Parse("8193:6160:9ba4:8599:d274:b058:4b02:3dff");

                var node1 = new NodeInfo { IpAddress = node1Ip };
                var node2 = new NodeInfo { IpAddress = node2Ip };

                serializer.Deserialize<NodeInfo>(Any<Stream>(), Any<CancellationToken>()).Returns(node1, node2);
                dnsService.GetIPAddresses(Any<string>(), Any<CancellationToken>()).Returns(new[] { node1Ip, node2Ip, node3Ip });

                handler
                .Expect(HttpMethod.Get, $"http://[{node1Ip}]/node")
                .Respond("application/json", JsonSerializer.Serialize(node1));

                handler
                .Expect(HttpMethod.Get, $"http://[{node2Ip}]/node")
                .Respond("application/json", JsonSerializer.Serialize(node2));

                var result = await service.GetPeers(node3Ip, cancellationToken);

                result.Should().ContainEquivalentOf(node1);
                result.Should().ContainEquivalentOf(node2);

                handler.VerifyNoOutstandingExpectation();
            }
        }
    }
}
