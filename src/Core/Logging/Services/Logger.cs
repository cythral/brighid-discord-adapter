using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord
{
    /// <summary>
    /// Logger class that uses category names based on the <see cref="LogCategoryAttribute" />.
    /// </summary>
    /// <typeparam name="TCategoryName">The category class / name to use.</typeparam>
    public class Logger<TCategoryName> : ILogger<TCategoryName>
    {
        private static readonly Dictionary<Type, string> Categories = new();
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger{TCategoryName}" /> class.
        /// </summary>
        /// <param name="factory">The logger factory to use.</param>
        public Logger([NotNull] ILoggerFactory factory)
        {
            if (!Categories.TryGetValue(typeof(TCategoryName), out var loggerName))
            {
                var attributes = typeof(TCategoryName).GetCustomAttributes(typeof(LogCategoryAttribute), true);
                loggerName = attributes.Any()
                    ? ((LogCategoryAttribute)attributes[0]).CategoryName
                    : typeof(TCategoryName).Name;

                Categories.Add(typeof(TCategoryName), loggerName);
            }

            logger = factory.CreateLogger(loggerName);
        }

        /// <inheritdoc />
        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return logger.BeginScope(state);
        }

        /// <inheritdoc />
        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return logger.IsEnabled(logLevel);
        }

        /// <inheritdoc />
        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
