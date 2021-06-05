using System.Diagnostics.CodeAnalysis;

using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.Commands
{
    /// <summary>
    /// Parser that parses messages into commands.
    /// </summary>
    public interface ICommandParser
    {
        /// <summary>
        /// Tries to parse a gateway message into a command.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <param name="options">Options to use for parsing messages into commands.</param>
        /// <param name="command">The parsed command, if successful, or null if not.</param>
        /// <returns>True if the message was successfully parsed as a command, or false if not.</returns>
        bool TryParseCommand(Message message, CommandParserOptions options, [MaybeNullWhen(false)] out Command? command);
    }
}
