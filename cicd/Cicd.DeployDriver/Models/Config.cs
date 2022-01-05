using System.Collections.Generic;

namespace Brighid.Discord.Cicd.DeployDriver
{
    /// <summary>
    /// Represents a template deployment config file.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Gets or sets the config file parameters.
        /// </summary>
        public Dictionary<string, string>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets the config file tags.
        /// </summary>
        public Dictionary<string, string>? Tags { get; set; }
    }
}
