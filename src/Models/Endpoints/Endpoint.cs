using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Represents an endpoint.
    /// </summary>
    [JsonConverter(typeof(EndpointConverter))]
    public struct Endpoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Endpoint" /> struct.
        /// </summary>
        /// <param name="category">The category of the endpoint.</param>
        /// <param name="enum">The endpoint enum value.</param>
        public Endpoint(char category, Enum @enum)
        {
            Category = category;
            Value = @enum;
        }

        /// <summary>
        /// Gets the endpoint category.
        /// </summary>
        public char Category { get; }

        /// <summary>
        /// Gets the endpoint value.
        /// </summary>
        public Enum Value { get; }

        /// <summary>
        /// Converts an enum into an endpoint.
        /// </summary>
        /// <param name="enum">The enum to convert.</param>
        public static implicit operator Endpoint(Enum @enum)
        {
            var attributes = @enum.GetType().GetCustomAttributes(typeof(ApiCategoryAttribute), true);
            if (attributes.Length == 0)
            {
                throw new Exception();
            }

            var categoryAttribute = (ApiCategoryAttribute)attributes[0];
            return new Endpoint(categoryAttribute.CategoryId, @enum);
        }

        /// <summary>
        /// Gets the endpoint information associated with the endpoint.
        /// </summary>
        /// <param name="enum">The enum to get endpoint info for.</param>
        /// <returns>The associated ApiEndpointAttribute the endpoint was annotated with.</returns>
        public static ApiEndpointAttribute? GetEndpointInfo(Enum @enum)
        {
            var memberInfo = @enum.GetType().GetMember(@enum.ToString()).FirstOrDefault();
            var attributes = from attr in memberInfo?.GetCustomAttributes(true) ?? Array.Empty<object>() where attr.GetType() == typeof(ApiEndpointAttribute) select attr;
            var endpointInfo = (ApiEndpointAttribute?)attributes.FirstOrDefault();
            return endpointInfo;
        }

        /// <summary>
        /// Gets the endpoint information associated with the endpoint.
        /// </summary>
        /// <returns>The associated ApiEndpointAttribute the endpoint was annotated with.</returns>
        public ApiEndpointAttribute? GetEndpointInfo()
        {
            return GetEndpointInfo(Value);
        }

        /// <summary>
        /// Gets the HttpMethod of an enum.
        /// </summary>
        /// <returns>The corresponding HttpMethod to this endpoint.</returns>
        public HttpMethod GetMethod()
        {
            var endpointInfo = GetEndpointInfo(Value);
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
