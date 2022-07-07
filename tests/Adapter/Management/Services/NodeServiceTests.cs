using System.Net.Http;
using System.Threading;

using Amazon.ECS;
using Amazon.ECS.Model;

using AutoFixture.NUnit3;

using FluentAssertions;

using NUnit.Framework;

using RichardSzalay.MockHttp;

using Task = System.Threading.Tasks.Task;

namespace Brighid.Discord.Adapter.Management
{
    public class NodeServiceTests
    {
        [Category("Unit")]
        public class GetDeploymentIdTests
        {
            [Test]
            [Auto]
            public async Task ShouldFetchNodeDeploymentId(
                string taskArn,
                string deploymentId,
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
                .Respond("application/json", $@"{{ ""TaskARN"": ""{taskArn}"" }}");

                describeResponse.Tasks[0].StartedBy = deploymentId;

                var result = await service.GetDeploymentId(cancellationToken);

                result.Should().Be(deploymentId);
                handler.VerifyNoOutstandingExpectation();
            }

            [Test]
            [Auto]
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
    }
}
