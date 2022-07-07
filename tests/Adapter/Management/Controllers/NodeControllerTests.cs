using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;

using NUnit.Framework;

namespace Brighid.Discord.Adapter.Management
{
    public class NodeControllerTests
    {
        [Category("Unit")]
        public class GetInfoTests
        {
            [Test]
            [Auto]
            public void ShouldReturnNodeInfo(
                [Frozen] NodeInfo info,
                [Target] NodeController controller
            )
            {
                var result = controller.GetInfo();

                result.Result.As<ObjectResult>().Value.Should().BeEquivalentTo(info);
            }

            [Test]
            [Auto]
            public void ShouldReturnOk(
                [Target] NodeController controller
            )
            {
                var result = controller.GetInfo();

                result.Result.Should().BeOfType<OkObjectResult>();
            }
        }
    }
}
