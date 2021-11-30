using System.Threading.Tasks;

using Brighid.Discord.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Brighid.Discord.RestClient.Responses
{
    /// <summary>
    /// Controller that handles responses to REST requests.
    /// </summary>
    [Route("/brighid/discord/rest-response")]
    public class ResponseController : Controller
    {
        private readonly IResponseService responseService;
        private readonly ILogger<ResponseController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseController" /> class.
        /// </summary>
        /// <param name="responseService">Service for handling responses.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public ResponseController(
            IResponseService responseService,
            ILogger<ResponseController> logger
        )
        {
            this.responseService = responseService;
            this.logger = logger;
        }

        /// <summary>
        /// Handles a response from the Brighid Discord Adapter.
        /// </summary>
        /// <param name="response">The response from the Discord API, relayed by the Discord Adapter.</param>
        /// <returns>OK if the response was handled successfully.</returns>
        [HttpPost]
        [Authorize(Roles = "DiscordResponder")]
        public Task<ActionResult> HandleResponse([FromBody] Response response)
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();
            logger.LogDebug("Response received for request {@requestId}", response.RequestId);
            responseService.HandleResponse(response);
            return Task.FromResult((ActionResult)Ok());
        }
    }
}
