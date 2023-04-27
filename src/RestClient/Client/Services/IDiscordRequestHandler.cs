using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.RestClient.Client
{
    /// <summary>
    /// Client used to queue Discord API Requests and wait for a response.
    /// </summary>
    public interface IDiscordRequestHandler
    {
        /// <summary>
        /// Queue a request.
        /// </summary>
        /// <typeparam name="TRequest">The type of request to queue.</typeparam>
        /// <param name="endpoint">Endpoint to send the request to.</param>
        /// <param name="request">The request to queue.</param>
        /// <param name="parameters">URL parameters to send with the request.</param>
        /// <param name="headers">Headers to send with the request.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Handle<TRequest>(Endpoint endpoint, TRequest request, Dictionary<string, string>? parameters = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queue a request and wait for its response.
        /// </summary>
        /// <typeparam name="TRequest">The type of request to send.</typeparam>
        /// <typeparam name="TResponse">The type of response to receive.</typeparam>
        /// <param name="endpoint">Endpoint to send the request to.</param>
        /// <param name="request">The request to queue.</param>
        /// <param name="parameters">URL parameters to send with the request.</param>
        /// <param name="headers">Headers to send with the request.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting response.</returns>
        Task<TResponse?> Handle<TRequest, TResponse>(Endpoint endpoint, TRequest request, Dictionary<string, string>? parameters = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queue a request without a body and wait for its response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to receive.</typeparam>
        /// <param name="endpoint">Endpoint to send the request to.</param>
        /// <param name="parameters">URL parameters to send with the request.</param>
        /// <param name="headers">Headers to send with the request.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting response.</returns>
        Task<TResponse?> Handle<TResponse>(Endpoint endpoint, Dictionary<string, string>? parameters = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    }
}
