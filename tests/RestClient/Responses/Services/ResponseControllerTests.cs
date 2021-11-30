using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.RestClient.Responses
{
    public class ResponseControllerTests
    {
        [TestFixture]
        [Category("Unit")]
        public class HandleResponseTests
        {
            [Test, Auto, Timeout(500)]
            public async Task ShouldHandleResponseWithResponseService(
                Response response,
                [Substitute] HttpContext httpContext,
                [Frozen, Substitute] IResponseService responseService,
                [Target] ResponseController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                await controller.HandleResponse(response);

                responseService.Received().HandleResponse(Is(response));
            }
        }
    }
}
