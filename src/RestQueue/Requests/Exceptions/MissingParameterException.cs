using System;

namespace Brighid.Discord.RestQueue.Requests
{
    /// <summary>
    /// Exception that is thrown when a URL parameter is missing from a template.
    /// </summary>
    public class MissingParameterException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingParameterException" /> class.
        /// </summary>
        /// <param name="parameter">Name of the missing parameter.</param>
        /// <param name="template">The template the parameter belongs to.</param>
        public MissingParameterException(string parameter, string template)
            : base($"Parameter {parameter} is missing from the URL Template: {template}")
        {
            Parameter = parameter;
            Template = template;
        }

        /// <summary>
        /// Gets or sets the name of the missing parameter.
        /// </summary>
        public string Parameter { get; set; }

        /// <summary>
        /// Gets or sets the name of the missing template.
        /// </summary>
        public string Template { get; set; }
    }
}
