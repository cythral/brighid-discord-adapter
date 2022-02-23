using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable SA1642

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Represents an API request to be made.
    /// </summary>
    public class Request
    {
        /// <summary>
        /// Initializes a new <see cref="Request" /> class.
        /// </summary>
        /// <param name="endpoint">The endpoint to use for the request.</param>
        public Request(
            Endpoint endpoint
        )
        {
            Endpoint = endpoint;
        }

        /// <summary>
        /// Gets or sets the request ID.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the trace header for this request.
        /// </summary>
        [JsonPropertyName("t")]
        public string? TraceHeader { get; set; }

        /// <summary>
        /// Gets or sets the path template to build the URL from.
        /// </summary>
        [JsonPropertyName("e")]
        public Endpoint Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the request body.
        /// </summary>
        [JsonPropertyName("b")]
        public string? RequestBody { get; set; }

        /// <summary>
        /// Gets or sets the response URL.
        /// </summary>
        [JsonPropertyName("r")]
        public Uri? ResponseURL { get; set; }

        /// <summary>
        /// Gets or sets the request parameters to be used in the endpoint's path template.
        /// </summary>
        [JsonPropertyName("p")]
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the headers to be sent along with the request.
        /// </summary>
        [JsonPropertyName("h")]
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }
}
