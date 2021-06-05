using System.Diagnostics.CodeAnalysis;

using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.Commands
{
    /// <inheritdoc />
    public class DefaultCommandParser : ICommandParser
    {
        /// <inheritdoc />
        public bool TryParseCommand(Message message, CommandParserOptions options, [MaybeNullWhen(false)] out Command? command)
        {
            var stateMachine = new CommandParserStateMachine(options);
            stateMachine.Run(message.Content);
            command = stateMachine.Success ? stateMachine.Result : null;
            return stateMachine.Success;
        }
    }
}
