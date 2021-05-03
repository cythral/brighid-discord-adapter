using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Brighid.Discord.GatewayAdapter.Gateway
{
    /// <inheritdoc />
    public class Channel<TMessage> : IChannel<TMessage>
    {
        private readonly System.Threading.Channels.Channel<TMessage> channel = Channel.CreateUnbounded<TMessage>();

        /// <inheritdoc />
        public async ValueTask<bool> WaitToRead(CancellationToken cancellationToken = default)
        {
            return await channel.Reader.WaitToReadAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async ValueTask<TMessage> Read(CancellationToken cancellationToken = default)
        {
            return await channel.Reader.ReadAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async ValueTask Write(TMessage message, CancellationToken cancellationToken)
        {
            await channel.Writer.WriteAsync(message, cancellationToken);
        }
    }
}
