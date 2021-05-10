using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Brighid.Discord.Threading
{
    /// <inheritdoc />
    public class Channel<TMessage> : IChannel<TMessage>
    {
        private readonly System.Threading.Channels.Channel<TMessage> channel = Channel.CreateUnbounded<TMessage>();
        private long messagesInQueue;

        /// <inheritdoc />
        public async ValueTask<bool> WaitToRead(CancellationToken cancellationToken = default)
        {
            return await channel.Reader.WaitToReadAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async ValueTask<TMessage> Read(CancellationToken cancellationToken = default)
        {
            Interlocked.Decrement(ref messagesInQueue);
            return await channel.Reader.ReadAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async ValueTask<bool> WaitToWrite(CancellationToken cancellationToken = default)
        {
            return await channel.Writer.WaitToWriteAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async ValueTask Write(TMessage message, CancellationToken cancellationToken)
        {
            await channel.Writer.WriteAsync(message, cancellationToken);
            Interlocked.Increment(ref messagesInQueue);
        }

        /// <inheritdoc />
        public async ValueTask<IEnumerable<TMessage>> ReadPending(uint max, FilterPredicate<TMessage>? filter = null, CancellationToken cancellationToken = default)
        {
            static async IAsyncEnumerable<TMessage> Enumerate(long max, IChannel<TMessage> reader, FilterPredicate<TMessage>? filter = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                for (var i = 0; i < max; i++)
                {
                    var message = await reader.Read(cancellationToken);
                    if (filter == null || await filter(message))
                    {
                        yield return message;
                    }
                }
            }

            var upperBound = Math.Min(messagesInQueue, max);
            return await Enumerate(upperBound, this, filter, cancellationToken)
                .ToListAsync(cancellationToken);
        }
    }
}
