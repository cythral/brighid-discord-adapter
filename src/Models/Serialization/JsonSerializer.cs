using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
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
        private readonly JsonSerializerContext serializerContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer" /> class.
        /// </summary>
        /// <param name="serializerContext">Serializer context to use.</param>
        /// <param name="converters">Converters to add to the json serializer context.</param>
        public JsonSerializer(
            JsonSerializerContext serializerContext,
            IEnumerable<JsonConverter> converters
        )
        {
            this.serializerContext = serializerContext;

            foreach (var converter in converters)
            {
                serializerContext.Options.Converters.Add(converter);
            }
        }

        /// <inheritdoc />
        public string Serialize<TSerializableType>(TSerializableType serializable)
        {
            var typeInfo = (JsonTypeInfo<TSerializableType>)(serializerContext.GetTypeInfo(typeof(TSerializableType)) ?? throw new Exception($"Could not find type info for type {typeof(TSerializableType).Name}."));
            return SystemTextJsonSerializer.Serialize(serializable, typeInfo);
        }

        /// <inheritdoc />
        public byte[] SerializeToBytes<TSerializableType>(TSerializableType serializable)
        {
            var result = Serialize(serializable);
            return Encoding.UTF8.GetBytes(result);
        }

        /// <inheritdoc />
        public TResultType? Deserialize<TResultType>(string deserializable)
        {
            var typeInfo = (JsonTypeInfo<TResultType>)(serializerContext.GetTypeInfo(typeof(TResultType)) ?? throw new Exception($"Could not find type info for type {typeof(TResultType).Name}."));
            return SystemTextJsonSerializer.Deserialize(deserializable, typeInfo);
        }

        /// <inheritdoc />
        public async Task<TResultType?> Deserialize<TResultType>(Stream deserializable, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var typeInfo = (JsonTypeInfo<TResultType>)(serializerContext.GetTypeInfo(typeof(TResultType)) ?? throw new Exception($"Could not find type info for type {typeof(TResultType).Name}."));
            return await SystemTextJsonSerializer.DeserializeAsync(deserializable, typeInfo, cancellationToken);
        }
    }
}
