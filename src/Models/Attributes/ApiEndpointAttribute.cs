using System;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Annotates an endpoint enum with it's associated path template.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ApiEndpointAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiEndpointAttribute" /> class.
        /// </summary>
        /// <param name="httpMethod">The http method to use.</param>
        /// <param name="template">The path template to use.</param>
        public ApiEndpointAttribute(string httpMethod, string template)
        {
            HttpMethod = httpMethod;
            Template = template;
        }

        /// <summary>
        /// Gets or sets the http method to use.
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// Gets or sets the path template to use.
        /// </summary>
        public string Template { get; set; }
    }
}
