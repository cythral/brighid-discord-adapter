using System;
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
        /// Gets or sets the response.
        /// </summary>
        [JsonPropertyName("m")]
        public string Message { get; set; }
    }
}
