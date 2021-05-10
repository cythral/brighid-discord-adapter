using System;
using System.Net;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// A response to a <see cref="Request" />.
    /// </summary>
    public struct Response
    {
        /// <summary>
        /// Gets or sets the request ID.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid RequestId { get; set; }

        /// <summary>
        /// Gets or sets the response status code.
        /// </summary>
        [JsonPropertyName("s")]
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        [JsonPropertyName("b")]
        public string? Body { get; set; }
    }
}
