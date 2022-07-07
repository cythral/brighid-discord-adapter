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

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeController"/> class.
        /// </summary>
        /// <param name="context">Adapter context.</param>
        public NodeController(
            IAdapterContext context
        )
        {
            this.context = context;
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
    }
}
