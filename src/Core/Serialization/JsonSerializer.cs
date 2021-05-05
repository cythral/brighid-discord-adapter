using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        private readonly JsonSerializerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer" /> class.
        /// </summary>
        /// <param name="converters">Converters to add to the serializer options.</param>
        public JsonSerializer(
            IEnumerable<JsonConverter> converters
        )
        {
            options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

            foreach (var converter in converters)
            {
                options.Converters.Add(converter);
            }
        }

        /// <summary>
        /// Serializes an object to JSON.
        /// </summary>
        /// <typeparam name="TSerializableType">The type of object to serialize to JSON.</typeparam>
        /// <param name="serializable">The object to serialize to JSON.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The serialized string.</returns>
        public async Task<string> Serialize<TSerializableType>(TSerializableType serializable, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = new JsonStringStream();
            await SystemTextJsonSerializer.SerializeAsync(stream, serializable, options, cancellationToken);
            return stream.Result;
        }

        /// <summary>
        /// Serializes an object to a JSON byte stream.
        /// </summary>
        /// <typeparam name="TSerializableType">The type of object to serialize to JSON.</typeparam>
        /// <param name="serializable">The object to serialize to JSON.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The serialized string.</returns>
        public async Task<byte[]> SerializeToBytes<TSerializableType>(TSerializableType serializable, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = new JsonByteStream();
            await SystemTextJsonSerializer.SerializeAsync(stream, serializable, options, cancellationToken);
            return stream.Result.ToArray();
        }

        /// <summary>
        /// Deserializes JSON into an object.
        /// </summary>
        /// <typeparam name="TResultType">The resulting object's type.</typeparam>
        /// <param name="deserializable">The JSON to deserialize.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The resulting object.</returns>
        public async Task<TResultType?> Deserialize<TResultType>(string deserializable, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytes = Encoding.UTF8.GetBytes(deserializable);
            using var stream = new MemoryStream(bytes);
            return await SystemTextJsonSerializer.DeserializeAsync<TResultType>(stream, options, cancellationToken);
        }

        /// <summary>
        /// Deserializes a stream containing JSON into an object.
        /// </summary>
        /// <typeparam name="TResultType">The resulting object's type.</typeparam>
        /// <param name="deserializable">The stream to deserialize from.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The resulting object.</returns>
        public async Task<TResultType?> Deserialize<TResultType>(Stream deserializable, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await SystemTextJsonSerializer.DeserializeAsync<TResultType>(deserializable, options, cancellationToken);
        }

#pragma warning disable IDE0060, SA1313, CA1822

        /// <summary>
        /// Implementation of a stream that, when written to, builds a list of bytes.  Reading and seeking
        /// via the conventional methods are not supported. Reading can be done by accessing the Result property directly.
        /// </summary>
        private class JsonByteStream : Stream
        {
            public List<byte> Result { get; private set; } = new();

            public override bool CanRead => false;

            public override bool CanWrite => true;

            public override bool CanSeek => false;

            public override long Length => Result.Count;

            public override long Position
            {
                get => Result.Count;
                set => throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                var span = buffer[offset..(offset + count)];
                Result.AddRange(span);
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
