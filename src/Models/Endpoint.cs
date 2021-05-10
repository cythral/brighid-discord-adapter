using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;

#pragma warning disable SA1649

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Represents an endpoint on the API that can be hit.
    /// </summary>
    public enum Endpoint
    {
        /// <summary>
        /// Endpoint to post a message to a channel.
        /// </summary>
        [ApiEndpoint(nameof(HttpMethod.Post), "/channels/{channel.id}/messages")]
        ChannelCreateMessage = 0,
    }

    /// <summary>
    /// Extension methods for the Endpoint enum.
    /// </summary>
    /// <todo>Auto-generate these.</todo>
    public static class EndpointExtensions
    {
        /// <summary>
        /// Gets the endpoint information associated with the endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to get info for.</param>
        /// <returns>The associated ApiEndpointAttribute the endpoint was annotated with.</returns>
        public static ApiEndpointAttribute? GetEndpointInfo(this Endpoint endpoint)
        {
            var memberInfo = typeof(Endpoint).GetMember(endpoint.ToString()).FirstOrDefault();
            var attributes = from attr in memberInfo?.GetCustomAttributes(true) ?? Array.Empty<object>() where attr.GetType() == typeof(ApiEndpointAttribute) select attr;
            var endpointInfo = (ApiEndpointAttribute?)attributes.FirstOrDefault();
            return endpointInfo;
        }

        /// <summary>
        /// Gets the HttpMethod of an enum.
        /// </summary>
        /// <param name="endpoint">The target endpoint.</param>
        /// <returns>The corresponding HttpMethod to this endpoint.</returns>
        public static HttpMethod GetMethod(this Endpoint endpoint)
        {
            var endpointInfo = endpoint.GetEndpointInfo();
            return endpointInfo?.HttpMethod switch
            {
                nameof(HttpMethod.Post) => HttpMethod.Post,
                nameof(HttpMethod.Get) => HttpMethod.Get,
                nameof(HttpMethod.Delete) => HttpMethod.Delete,
                nameof(HttpMethod.Put) => HttpMethod.Put,
                nameof(HttpMethod.Patch) => HttpMethod.Patch,
                nameof(HttpMethod.Head) => HttpMethod.Head,
                _ => throw new InvalidEnumArgumentException(),
            };
        }
    }
}
