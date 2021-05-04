using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Represents an API request to be made.
    /// </summary>
    public struct Request
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Request" /> struct.
        /// </summary>
        /// <param name="endpoint">The endpoint to use for the request.</param>
        public Request(Endpoint endpoint)
        {
            Id = Guid.NewGuid();
            Endpoint = endpoint;
            RequestBody = null;
            ResponseQueueURL = null;
            Parameters = new Dictionary<string, string>();
            Headers = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets the request ID.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

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
        /// Gets or sets the response queue URL.
        /// </summary>
        [JsonPropertyName("r")]
        public Uri? ResponseQueueURL { get; set; }

        /// <summary>
        /// Gets or sets the request parameters to be used in the endpoint's path template.
        /// </summary>
        [JsonPropertyName("p")]
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the headers to be sent along with the request.
        /// </summary>
        [JsonPropertyName("h")]
        public Dictionary<string, string> Headers { get; set; }
    }
}
