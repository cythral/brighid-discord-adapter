using System;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.RestClient.Responses
{
    /// <summary>
    /// Service that abstracts listening for a response.
    /// </summary>
    public interface IResponseService
    {
        /// <summary>
        /// Gets the internally-accessible uri the server can be reached at.
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// Handles a response from the REST Queue.
        /// </summary>
        /// <param name="response">The response from the rest queue.</param>
        void HandleResponse(Response response);

        /// <summary>
        /// Listens for a response to the given requestID.
        /// </summary>
        /// <param name="requestId">ID of the request to listen for a response for.</param>
        /// <param name="promise">Promise that will be completed when the response comes back.</param>
        /// <returns>The response.</returns>
        Task<Response> ListenForResponse(Guid requestId, TaskCompletionSource<Response> promise);
    }
}
