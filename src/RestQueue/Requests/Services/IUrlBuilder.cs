using System;

using Brighid.Discord.Models;

namespace Brighid.Discord.RestQueue.Requests
{
    /// <summary>
    /// Utility to build URLs from requests.
    /// </summary>
    public interface IUrlBuilder
    {
        /// <summary>
        /// Builds a URL from a request.
        /// </summary>
        /// <param name="request">The request to build a URL for.</param>
        /// <returns>The resulting URL.</returns>
        Uri BuildFromRequest(Request request);
    }
}
