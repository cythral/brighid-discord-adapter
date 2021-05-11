using System;

using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// Exception that gets thrown when supplied an Endpoint that is not yet handled by the API.
    /// </summary>
    public class UnhandledEndpointException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnhandledEndpointException" /> class.
        /// </summary>
        /// <param name="endpoint">The endpoint that is unhandled.</param>
        public UnhandledEndpointException(Endpoint endpoint)
            : base($"Unknown endpoint: {endpoint}")
        {
            Endpoint = endpoint;
        }

        /// <summary>
        /// Gets or sets the unhandled endpoint.
        /// </summary>
        public Endpoint Endpoint { get; set; }
    }
}
