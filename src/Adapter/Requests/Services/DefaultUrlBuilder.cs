using System;

using Brighid.Discord.Adapter.Management;
using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.Requests
{
    /// <inheritdoc />
    public class DefaultUrlBuilder : IUrlBuilder
    {
        private readonly IDiscordApiInfoService discordApiInfoService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultUrlBuilder" /> class.
        /// </summary>
        /// <param name="discordApiInfoService">Discord API Info Service.</param>
        public DefaultUrlBuilder(
            IDiscordApiInfoService discordApiInfoService
        )
        {
            this.discordApiInfoService = discordApiInfoService;
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

            return new Uri(discordApiInfoService.ApiBaseUrl + result);
        }

        private static bool IsParameter(string value, out string name)
        {
            var result = value.StartsWith('{') && value.EndsWith('}');
            name = result ? value.TrimStart('{').TrimEnd('}') : string.Empty;
            return result;
        }
    }
}
