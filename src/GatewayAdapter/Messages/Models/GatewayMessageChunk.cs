using System;
using System.Text;

namespace Brighid.Discord.GatewayAdapter.Messages
{
    /// <summary>
    /// A message chunk received from the gateway.
    /// </summary>
    public struct GatewayMessageChunk
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayMessageChunk" /> struct.
        /// </summary>
        /// <param name="buffer">The buffer used for the chunk.</param>
        /// <param name="count">The number of bytes in the chunk.</param>
        /// <param name="endOfMessage">Whether this chunk is the end of the message.</param>
        public GatewayMessageChunk(Memory<byte> buffer, int count, bool endOfMessage)
        {
            Bytes = new Memory<byte>(new byte[count]);
            Count = count;
            EndOfMessage = endOfMessage;

            buffer.Slice(0, count).CopyTo(Bytes);
        }

        /// <summary>
        /// Gets the message bytes.
        /// </summary>
        public Memory<byte> Bytes { get; init; }

        /// <summary>
        /// Gets the byte count of the message.
        /// </summary>
        public int Count { get; init; }

        /// <summary>
        /// Gets a value indicating whether this chunk is the end of the message.
        /// </summary>
        public bool EndOfMessage { get; init; }

        /// <summary>
        /// Converts the chunk to a string.
        /// </summary>
        /// <returns>The resulting string representation of this chunk.</returns>
        public override string ToString()
        {
            return Encoding.UTF8.GetString(Bytes.ToArray());
        }
    }
}
