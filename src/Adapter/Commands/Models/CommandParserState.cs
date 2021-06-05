namespace Brighid.Discord.Adapter.Commands
{
    /// <summary>
    /// A state of the Command Parser.
    /// </summary>
    internal enum CommandParserState
    {
        /// <summary>
        /// State that handles prefixes.
        /// </summary>
        Prefix,

        /// <summary>
        /// State that handles command names.
        /// </summary>
        CommandName,

        /// <summary>
        /// State that handles arguments.
        /// </summary>
        Arguments,

        /// <summary>
        /// State that handles option names.
        /// </summary>
        OptionName,

        /// <summary>
        /// State that handles option values.
        /// </summary>
        OptionValue,

        /// <summary>
        /// Ending state.
        /// </summary>
        End,
    }
}
