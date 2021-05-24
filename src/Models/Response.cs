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
        /// Initializes a new instance of the <see cref="Response" /> struct.
        /// </summary>
        /// <param name="requestId">ID of the request.</param>
        /// <param name="statusCode">Status code of the response.</param>
        /// <param name="body">The body of the response.</param>
        public Response(
            Guid requestId,
            HttpStatusCode statusCode,
            string body
        )
        {
            RequestId = requestId;
            StatusCode = statusCode;
            Body = body;
        }

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
