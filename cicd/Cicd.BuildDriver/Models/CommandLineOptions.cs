using System.ComponentModel.DataAnnotations;

namespace Brighid.Discord.Cicd.Driver
{
    /// <summary>
    /// Options presented on the command line.
    /// </summary>
    public class CommandLineOptions
    {
        /// <summary>
        /// Gets or sets the version to build.
        /// </summary>
        [Required]
        public string Version { get; set; } = string.Empty;
    }
}
