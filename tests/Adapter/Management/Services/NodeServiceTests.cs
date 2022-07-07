using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

using Amazon.ECS;
using Amazon.ECS.Model;

using AutoFixture.NUnit3;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using RichardSzalay.MockHttp;

using static NSubstitute.Arg;

using Task = System.Threading.Tasks.Task;

namespace Brighid.Discord.Adapter.Management
{
    public class NodeServiceTests
    {
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
                [Frozen] IAmazonECS ecs,
                [Frozen] MockHttpMessageHandler handler,
                [Target] NodeService service,
                CancellationToken cancellationToken
            )
            {
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
                NodeInfo node1,
                NodeInfo node2,
                IPAddress node1Ip,
                IPAddress node2Ip,
                [Frozen] IDnsService dnsService,
                [Frozen] MockHttpMessageHandler handler,
                [Target] NodeService service,
                CancellationToken cancellationToken
            )
            {
                dnsService.GetIPAddresses(Any<string>(), Any<CancellationToken>()).Returns(new[] { node1Ip, node2Ip });

                handler
                .Expect(HttpMethod.Get, $"http://{node1Ip}/node")
                .Respond("application/json", JsonSerializer.Serialize(node1));

                handler
                .Expect(HttpMethod.Get, $"http://{node2Ip}/node")
                .Respond("application/json", JsonSerializer.Serialize(node2));

                var result = await service.GetPeers(cancellationToken);

                result.Should().ContainEquivalentOf(node1);
                result.Should().ContainEquivalentOf(node2);

                handler.VerifyNoOutstandingExpectation();
            }
        }
    }
}
