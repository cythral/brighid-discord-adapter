using System;

using Brighid.Discord.Models;

using Microsoft.Extensions.Options;

namespace Brighid.Discord.Adapter.Requests
{
    /// <inheritdoc />
    public class DefaultUrlBuilder : IUrlBuilder
    {
        private readonly RequestOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultUrlBuilder" /> class.
        /// </summary>
        /// <param name="options">Options to use when making requests.</param>
        public DefaultUrlBuilder(
            IOptions<RequestOptions> options
        )
        {
            this.options = options.Value;
        }

        /// <inheritdoc />
        public Uri BuildFromRequest(Request request)
        {
            var endpointInfo = request.Endpoint.GetEndpointInfo();
            var result = string.Empty;

            if (endpointInfo?.Template == null)
            {
                throw new UnhandledEndpointException(request.Endpoint);
            }

            var templateParts = endpointInfo.Template.TrimStart('/').Split('/');
            foreach (var part in templateParts)
            {
                string? parameterValue = null;
                if (IsParameter(part, out var parameterName) && !request.Parameters.TryGetValue(parameterName, out parameterValue))
                {
                    throw new MissingParameterException(parameterName, endpointInfo.Template);
                }

                result += parameterValue != null
                    ? $"/{parameterValue}"
                    : $"/{part}";
            }

            return new Uri(options.InvokeBaseUrl + result);
        }

        private bool IsParameter(string value, out string name)
        {
            var result = value.StartsWith('{') && value.EndsWith('}');
            name = result ? value.TrimStart('{').TrimEnd('}') : string.Empty;
            return result;
        }
    }
}
