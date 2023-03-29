using System.Collections.Generic;
using System.Threading.Tasks;

using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Gateway;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Management
{
    public class NodeControllerTests
    {
        [Category("Unit")]
        public class GetInfoTests
        {
            [Test, Auto]
            public void ShouldReturnNodeInfo(
                [Frozen] NodeInfo info,
                [Target] NodeController controller
            )
            {
                var result = controller.GetInfo();

                result.Result.As<ObjectResult>().Value.Should().BeEquivalentTo(info);
            }

            [Test, Auto]
            public void ShouldReturnOk(
                [Target] NodeController controller
            )
            {
                var result = controller.GetInfo();

                result.Result.Should().BeOfType<OkObjectResult>();
            }
        }

        [Category("Unit")]
        public class GetPeersTests
        {
            [Test, Auto]
            public void ShouldReturnPeers(
                [Frozen] IEnumerable<NodeInfo> peers,
                [Target] NodeController controller
            )
            {
                var result = controller.GetPeers();

                result.Result.As<ObjectResult>().Value.Should().BeEquivalentTo(peers);
            }
        }

        [Category("Unit")]
        public class GetGatewayStatusTests
        {
            [Test, Auto]
            public void ShouldReturnTheGatewayStatus(
                [Frozen] GatewayState state,
                [Target] NodeController controller
            )
            {
                var result = controller.GetGatewayState();

                result.Result.As<ObjectResult>().Value.Should().BeEquivalentTo(state.ToString());
            }

            [Test, Auto]
            public void ShouldReturnOk(
                [Target] NodeController controller
            )
            {
                var result = controller.GetGatewayState();

                result.Result.Should().BeOfType<OkObjectResult>();
            }
        }

        [Category("Unit")]
        public class GetGatewayRxTaskCountTests
        {
            [Test, Auto]
            public void ShouldReturnRxTaskCountQueue(
                int count,
                [Frozen] IGatewayService gateway,
                [Target] NodeController controller
            )
            {
                gateway.RxTaskCount.Returns(count);
                var result = controller.GetGatewayRxTaskCount();

                result.Result.As<ObjectResult>().Value.Should().BeEquivalentTo(count.ToString());
            }

            [Test, Auto]
            public void ShouldReturnOk(
                [Target] NodeController controller
            )
            {
                var result = controller.GetGatewayRxTaskCount();

                result.Result.Should().BeOfType<OkObjectResult>();
            }
        }

        [Category("Unit")]
        public class RestartTests
        {
            [Test, Auto]
            public async Task ShouldRestartTheGateway(
                HttpContext httpContext,
                [Frozen] IGatewayService gateway,
                [Target] NodeController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                await controller.Restart();

                await gateway.Received().Restart(Is(true), Is(httpContext.RequestAborted));
            }

            [Test, Auto]
            public async Task ShouldReturnOk(
                HttpContext httpContext,
                [Frozen] IGatewayService gateway,
                [Target] NodeController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var result = await controller.Restart();

                result.Should().BeOfType<NoContentResult>();
            }
        }

        [Category("Unit")]
        public class SetGatewayStateTests
        {
            [Test, Auto]
            public async Task ShouldStopTheGateway(
                HttpContext httpContext,
                [Frozen] IGatewayService gateway,
                [Target] NodeController controller
            )
            {
                gateway.State.Returns(GatewayState.Running);
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                await controller.SetGatewayState(GatewayState.Stopped);

                await gateway.Received().StopAsync(Is(httpContext.RequestAborted));
            }

            [Test, Auto]
            public async Task ShouldNotStopTheGatewayIfItsAlreadyStopped(
                HttpContext httpContext,
                [Frozen] IGatewayService gateway,
                [Target] NodeController controller
            )
            {
                gateway.State.Returns(GatewayState.Stopped);
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                await controller.SetGatewayState(GatewayState.Stopped);

                await gateway.DidNotReceive().StopAsync(Is(httpContext.RequestAborted));
            }

            [Test, Auto]
            public async Task ShouldStartTheGateway(
                HttpContext httpContext,
                [Frozen] IGatewayService gateway,
                [Target] NodeController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                await controller.SetGatewayState(GatewayState.Running);

                await gateway.Received().StartAsync(Is(httpContext.RequestAborted));
            }

            [Test, Auto]
            public async Task ShouldNotStartTheGatewayIfItsAlreadyRunning(
                HttpContext httpContext,
                [Frozen] IGatewayService gateway,
                [Target] NodeController controller
            )
            {
                gateway.State.Returns(GatewayState.Running | GatewayState.Ready);
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                await controller.SetGatewayState(GatewayState.Running);

                await gateway.DidNotReceive().StartAsync(Is(httpContext.RequestAborted));
            }

            [Test, Auto]
            public async Task ShouldReturnNoContent(
                HttpContext httpContext,
                [Frozen] IGatewayService gateway,
                [Target] NodeController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var result = await controller.SetGatewayState(GatewayState.Stopped);

                result.Should().BeOfType<NoContentResult>();
            }

            [Test, Auto]
            public async Task ShouldNotStopTheGatewayIfGivenStateIsNotStopped(
                HttpContext httpContext,
                [Frozen] IGatewayService gateway,
                [Target] NodeController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                await controller.SetGatewayState(GatewayState.Running);

                await gateway.DidNotReceive().StopAsync(Is(httpContext.RequestAborted));
            }

            [Test, Auto]
            public async Task ShouldReturnBadRequestIfGivenStateIsReady(
                HttpContext httpContext,
                [Frozen] IGatewayService gateway,
                [Target] NodeController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var result = await controller.SetGatewayState(GatewayState.Ready);

                result.Should().BeOfType<BadRequestObjectResult>();
            }
        }
    }
}
