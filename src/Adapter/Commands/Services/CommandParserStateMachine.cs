using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Brighid.Discord.Adapter.Commands
{
    /// <summary>
    /// State Machine used for parsing commands.
    /// </summary>
    internal class CommandParserStateMachine
    {
        private readonly List<string> arguments = new();
        private readonly CommandParserOptions options;
        private string currentArg = string.Empty;
        private string currentOptionName = string.Empty;
        private string currentOptionValue = string.Empty;
        private CommandParserState state;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandParserStateMachine" /> class.
        /// </summary>
        /// <param name="options">Options to use when parsing.</param>
        public CommandParserStateMachine(
            CommandParserOptions options
        )
        {
            this.options = options;
        }

        /// <summary>
        /// Gets a value indicating whether the parse was successful.
        /// </summary>
        public bool Success { get; private set; } = true;

        /// <summary>
        /// Gets the result of the parsing (should not be used if Success is false).
        /// </summary>
        public Command Result { get; } = new Command();

        /// <summary>
        /// Runs the state machine.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        public void Run(string message)
        {
            var charEnumerator = message.GetEnumerator();

            while (charEnumerator.MoveNext() && state != CommandParserState.End)
            {
                var current = charEnumerator.Current;

                switch (state)
                {
                    case CommandParserState.Prefix: HandlePrefixState(current); break;
                    case CommandParserState.CommandName: HandleCommandNameState(current); break;
                    case CommandParserState.Arguments: HandleArgumentsState(current); break;
                    case CommandParserState.OptionName: HandleOptionNameState(current); break;
                    case CommandParserState.OptionValue: HandleOptionValueState(current); break;
                    case CommandParserState.End: default: break;
                }
            }

            if (currentArg != string.Empty)
            {
                if (arguments.Count >= options.ArgLimit)
                {
                    Success = false;
                }
                else
                {
                    arguments.Add(currentArg);
                }
            }

            if (currentOptionName != string.Empty)
            {
                object value = (currentOptionValue != string.Empty)
                    ? currentOptionValue
                    : true;

                Result.Options.Add(currentOptionName, value);
            }

            if (Success)
            {
                Result.Arguments = arguments.ToArray();
            }
        }

        /// <summary>
        /// Handles the prefix state.
        /// </summary>
        /// <param name="input">The character input for the prefix state.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HandlePrefixState(char input)
        {
            if (input != options.Prefix)
            {
                Success = false;
                state = CommandParserState.End;
                return;
            }

            state = CommandParserState.CommandName;
        }

        /// <summary>
        /// Handles the command name state.
        /// </summary>
        /// <param name="input">The character input for the command name state.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HandleCommandNameState(char input)
        {
            if (input == ' ')
            {
                state = CommandParserState.Arguments;
                return;
            }

            if (Result.Name == string.Empty && !char.IsLetterOrDigit(input))
            {
                Success = false;
                state = CommandParserState.End;
            }

            Result.Name += input;
        }

        /// <summary>
        /// Handles the arguments state.
        /// </summary>
        /// <param name="input">The character input for the arguments state.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HandleArgumentsState(char input)
        {
            if (input == options.ArgSeparator && (options.ArgLimit == 0 || arguments.Count < options.ArgLimit - 1))
            {
                if (currentArg != string.Empty)
                {
                    arguments.Add(currentArg);
                }

                currentArg = string.Empty;
                return;
            }

            currentArg += input;

            if (currentArg.StartsWith(options.OptionPrefix) || currentArg.EndsWith($" {options.OptionPrefix}"))
            {
                state = CommandParserState.OptionName;
                currentOptionName = string.Empty;
                currentOptionValue = string.Empty;
                return;
            }
        }

        /// <summary>
        /// Handles the option name state.
        /// </summary>
        /// <param name="input">The character input for the arguments state.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HandleOptionNameState(char input)
        {
            if (currentOptionName == string.Empty && !char.IsLetterOrDigit(input))
            {
                state = CommandParserState.Arguments;
                currentOptionName = string.Empty;
                HandleArgumentsState(input);
                return;
            }

            if (input == ' ')
            {
                var arg = currentArg.Length >= options.OptionPrefix.Length
                    ? currentArg[0..(currentArg.Length - options.OptionPrefix.Length)].Trim()
                    : string.Empty;

                if (arg != string.Empty)
                {
                    arguments.Add(arg);
                }

                currentArg = string.Empty;
                state = CommandParserState.OptionValue;
                return;
            }

            currentOptionName += input;
        }

        /// <summary>
        /// Handles the option value state.
        /// </summary>
        /// <param name="input">The character input for the arguments state.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HandleOptionValueState(char input)
        {
            if (input == ' ')
            {
                Result.Options[currentOptionName] = currentOptionValue;
                state = CommandParserState.Arguments;
                currentArg = string.Empty;
                currentOptionName = string.Empty;
                currentOptionValue = string.Empty;
                return;
            }

            currentOptionValue += input;

            if (currentOptionValue == options.OptionPrefix)
            {
                Result.Options[currentOptionName] = true;
                state = CommandParserState.Arguments;
                currentArg = string.Empty;
                currentOptionName = string.Empty;
                currentOptionValue = string.Empty;
                return;
            }
        }
    }
}
