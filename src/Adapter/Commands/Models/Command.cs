using System;
using System.Collections.Generic;

namespace Brighid.Discord.Adapter.Commands
{
    /// <summary>
    /// Represents a command that a user sent as a message.
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Gets or sets the command name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the command arguments.
        /// </summary>
        public string[] Arguments { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the command options.
        /// </summary>
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
    }
}
