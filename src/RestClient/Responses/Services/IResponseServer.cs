using System;
using System.Threading.Tasks;

using Brighid.Discord.Models;

using Microsoft.Extensions.Hosting;

namespace Brighid.Discord.RestClient.Responses
{
    /// <summary>
    /// Server that listens for responses to REST API requests from the queue worker.
    /// </summary>
    public interface IResponseServer : IHostedService
    {
        /// <summary>
        /// Gets a value indicating whether the response server is running or not.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the internally-accessible uri the server can be reached at.
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// Listens for a response to the given requestID.
        /// </summary>
        /// <param name="requestId">ID of the request to listen for a response for.</param>
        /// <param name="promise">Promise that will be completed when the response comes back.</param>
        /// <returns>The response.</returns>
        Task<Response> ListenForResponse(Guid requestId, TaskCompletionSource<Response> promise);
    }
}
