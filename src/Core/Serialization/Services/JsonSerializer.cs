using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SystemTextJsonSerializer = System.Text.Json.JsonSerializer;

namespace Brighid.Discord.Serialization
{
    /// <summary>
    /// Serializer that serializes to/from JSON.
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        /// <summary>
        /// Serializes an object to JSON.
        /// </summary>
        /// <typeparam name="TSerializableType">The type of object to serialize to JSON.</typeparam>
        /// <param name="serializable">The object to serialize to JSON.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The serialized string.</returns>
        public async Task<string> Serialize<TSerializableType>(TSerializableType serializable, CancellationToken cancellationToken)
        {
            using var stream = new JsonStringStream();
            await SystemTextJsonSerializer.SerializeAsync(stream, serializable, cancellationToken: cancellationToken);
            return stream.Result;
        }

        /// <summary>
        /// Deserializes JSON into an object.
        /// </summary>
        /// <typeparam name="TResultType">The resulting object's type.</typeparam>
        /// <param name="deserializable">The JSON to deserialize.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The resulting type.</returns>
        public async Task<TResultType?> Deserialize<TResultType>(string deserializable, CancellationToken cancellationToken)
        {
            var bytes = Encoding.UTF8.GetBytes(deserializable);
            using var stream = new MemoryStream(bytes);
            return await SystemTextJsonSerializer.DeserializeAsync<TResultType>(stream, cancellationToken: cancellationToken);
        }

#pragma warning disable IDE0060, SA1313, CA1822

        /// <summary>
        /// Implementation of a stream that, when written to, builds a string.  Reading and seeking
        /// via the conventional methods are not supported. Reading can be done by accessing the Result property directly.
        /// </summary>
        private class JsonStringStream : Stream
        {
            public string Result { get; private set; } = string.Empty;

            public override bool CanRead => false;

            public override bool CanWrite => true;

            public override bool CanSeek => false;

            public override long Length => Result.Length;

            public override long Position
            {
                get => Result.Length;
                set => throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                var span = buffer[offset..(offset + count)];
                var spanString = Encoding.UTF8.GetString(span);
                Result += spanString;
            }

            public override int Read(byte[] _buffer, int _offset, int _count)
            {
                throw new NotSupportedException();
            }

            public override long Seek(long _offset, SeekOrigin _origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long _value)
            {
            }

            public override void Flush()
            {
            }
        }
    }
}
