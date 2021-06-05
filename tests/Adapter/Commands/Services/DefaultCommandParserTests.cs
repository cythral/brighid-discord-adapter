using System;

using Brighid.Discord.Models;

using FluentAssertions;

using NUnit.Framework;

namespace Brighid.Discord.Adapter.Commands
{
    public class DefaultCommandParserTests
    {
        [TestFixture]
        public class TryParseCommandTests
        {
            [Test, Auto]
            public void ShouldReturnFalseWhenNotPrefixed(
                CommandParserOptions options,
                [Target] DefaultCommandParser parser
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';

                var message = new Message { Content = "echo Hello World" };
                var result = parser.TryParseCommand(message, options, out var command);

                result.Should().BeFalse();
                command.Should().BeNull();
            }

            [Test, Auto]
            public void ShouldReturnFalseWhenCommandNameDoesntStartWithALetter(
                CommandParserOptions options,
                [Target] DefaultCommandParser parser
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';

                var message = new Message { Content = "..echo Hello World" };
                var result = parser.TryParseCommand(message, options, out var command);

                result.Should().BeFalse();
                command.Should().BeNull();
            }

            [Test, Auto]
            public void ShouldParseCommandNameWhenPrefixed(
                CommandParserOptions options,
                [Target] DefaultCommandParser parser
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';

                var message = new Message { Content = ".echo Hello World" };
                var result = parser.TryParseCommand(message, options, out var command);

                result.Should().BeTrue();
                command!.Name.Should().Be("echo");
            }

            [Test, Auto]
            public void ShouldParseCommandArgumentsWhenPrefixed(
                CommandParserOptions options,
                [Target] DefaultCommandParser parser
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';

                var message = new Message { Content = ".echo Hello World" };
                var result = parser.TryParseCommand(message, options, out var command);

                result.Should().BeTrue();
                command!.Arguments.Should().BeEquivalentTo(new[] { "Hello", "World" });
            }

            [Test, Auto]
            public void ShouldParseCommandsWithNoArguments(
                CommandParserOptions options,
                [Target] DefaultCommandParser parser
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';

                var message = new Message { Content = ".ping" };
                var result = parser.TryParseCommand(message, options, out var command);

                result.Should().BeTrue();
                command!.Name.Should().Be("ping");
                command!.Arguments.Should().BeEquivalentTo(Array.Empty<string>());
            }

            [Test, Auto]
            public void ShouldCombineArgumentsIntoTheLastArgIfArgLimitIsReached(
                CommandParserOptions options,
                [Target] DefaultCommandParser parser
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';
                options.ArgLimit = 1;

                var message = new Message { Content = ".echo This is a lot of arguments" };
                var result = parser.TryParseCommand(message, options, out var command);

                result.Should().BeTrue();
                command!.Name.Should().Be("echo");
                command!.Arguments.Should().BeEquivalentTo(new[] { "This is a lot of arguments" });
            }

            [Test, Auto]
            public void ShouldParseOptions(
                CommandParserOptions options,
                [Target] DefaultCommandParser parser
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';
                options.OptionPrefix = "--";
                options.ArgLimit = 1;

                var message = new Message { Content = ".echo This is a lot of arguments --hello world --foo bar" };
                var result = parser.TryParseCommand(message, options, out var command);

                result.Should().BeTrue();
                command!.Arguments.Should().BeEquivalentTo(new[] { "This is a lot of arguments" });
                command!.Options.Should().Contain("hello", "world");
                command!.Options.Should().Contain("foo", "bar");
            }

            [Test, Auto]
            public void ShouldReturnFalseIfArgumentsExceedMaxArgsAroundOptions(
                CommandParserOptions options,
                [Target] DefaultCommandParser parser
            )
            {
                options.Prefix = '.';
                options.ArgSeparator = ' ';
                options.OptionPrefix = "--";
                options.ArgLimit = 1;

                var message = new Message { Content = ".echo This is a lot of arguments --hello world --foo bar more arguments here" };
                var result = parser.TryParseCommand(message, options, out var _);

                result.Should().BeFalse();
            }
        }
    }
}
