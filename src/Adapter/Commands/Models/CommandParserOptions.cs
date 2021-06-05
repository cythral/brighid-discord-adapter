namespace Brighid.Discord.Adapter.Commands
{
    /// <summary>
    /// Options to use when parsing messages as commands.
    /// </summary>
    public class CommandParserOptions
    {
        /// <summary>
        /// Gets or sets the prefix to use for commands.
        /// </summary>
        public char Prefix { get; set; } = '.';

        /// <summary>
        /// Gets or sets the argument separator.
        /// </summary>
        public char ArgSeparator { get; set; } = ' ';

        /// <summary>
        /// Gets or sets the maximum amount of arguments the command may have.
        /// </summary>
        public uint ArgLimit { get; set; } = 0;

        /// <summary>
        /// Gets or sets the prefix used for option flags.
        /// </summary>
        public string OptionPrefix { get; set; } = "--";
    }
}
