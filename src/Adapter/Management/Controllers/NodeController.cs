using System;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Gateway;

using Microsoft.AspNetCore.Mvc;

namespace Brighid.Discord.Adapter.Management
{
    /// <summary>
    /// Controller for node-specific endpoints.
    /// </summary>
    [Route("/node")]
    public class NodeController : Controller
    {
        private readonly IAdapterContext context;
        private readonly IGatewayService gateway;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeController"/> class.
        /// </summary>
        /// <param name="context">Adapter context.</param>
        /// <param name="gateway">Gateway service.</param>
        public NodeController(
            IAdapterContext context,
            IGatewayService gateway
        )
        {
            this.context = context;
            this.gateway = gateway;
        }

        /// <summary>
        /// Gets info about the current node.
        /// </summary>
        /// <returns>The node info.</returns>
        [HttpGet]
        public ActionResult<NodeInfo> GetInfo()
        {
            return Ok(context.Get<NodeInfo>());
        }

        /// <summary>
        /// Gets the current state of the gateway.
        /// </summary>
        /// <returns>The current state of the gateway.</returns>
        [HttpGet("gateway/state")]
        [Produces("text/plain", Type = typeof(GatewayState))]
        public ActionResult<string> GetGatewayState()
        {
            return Ok(gateway.State.ToString());
        }

        /// <summary>
        /// Sets the current state of the gateway. Currently only supports setting gateway state to false.
        /// </summary>
        /// <param name="state">The desired state of the gateway.</param>
        /// <returns>A no content result if successful.</returns>
        // [Authorize(Roles = "DiscordNodeManager")
        [HttpPut("gateway/state")]
        [Consumes("text/plain"), Produces("text/plain")]
        public async Task<ActionResult> SetGatewayState([FromBody] GatewayState state)
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();

            if ((state == GatewayState.Stopped && gateway.State == GatewayState.Stopped) || (state != GatewayState.Stopped && gateway.State.HasFlag(state)))
            {
                return NoContent();
            }

            try
            {
                Func<CancellationToken, Task> func = state switch
                {
                    GatewayState.Stopped => gateway.StopAsync,
                    GatewayState.Running => gateway.StartAsync,
                    GatewayState.Ready or _ => throw new InvalidOperationException($"Given state {state} is not allowed.  Valid values: {nameof(GatewayState.Stopped)}, {nameof(GatewayState.Running)}"),
                };

                await func(HttpContext.RequestAborted);
                return NoContent();
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
            }
        }
    }
}
