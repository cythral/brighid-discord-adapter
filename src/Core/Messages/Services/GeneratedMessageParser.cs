#pragma warning disable CS0535
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Serialization;

/* You can ignore errors in this file, as the rest of the class is auto-generated. */

namespace Brighid.Discord.Messages
{
    /// <inheritdoc />
    public partial class GeneratedMessageParser : IMessageParser
    {
        private readonly ISerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedMessageParser" /> class.
        /// </summary>
        /// <param name="serializer">The serializer to serialize/deserialize messages with.</param>
        public GeneratedMessageParser(
            ISerializer serializer
        )
        {
            this.serializer = serializer;
        }

        /// <inheritdoc />
        public async Task<GatewayMessage> Parse(Stream stream, CancellationToken cancellationToken)
        {
            var messageWithoutData = await serializer.Deserialize<GatewayMessageWithoutData>(stream, cancellationToken);
            return await ParseEventData(messageWithoutData!, cancellationToken);
        }
    }
}
