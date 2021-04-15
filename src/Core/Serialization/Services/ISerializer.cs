using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Serialization
{
    /// <summary>
    /// Allows for serializing an object in memory to a text format that can be sent across a network.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serializes an object to a text-based format.
        /// </summary>
        /// <typeparam name="TSerializableType">The type of object to serialize.</typeparam>
        /// <param name="serializable">The object to serialize.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The serialized string.</returns>
        Task<string> Serialize<TSerializableType>(TSerializableType serializable, CancellationToken cancellationToken);

        /// <summary>
        /// Deserializes text into an object.
        /// </summary>
        /// <typeparam name="TResultType">The resulting object's type.</typeparam>
        /// <param name="deserializable">The text to deserialize.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The resulting object.</returns>
        Task<TResultType?> Deserialize<TResultType>(string deserializable, CancellationToken cancellationToken);

        /// <summary>
        /// Deserializes a stream into an object.
        /// </summary>
        /// <typeparam name="TResultType">The resulting object's type.</typeparam>
        /// <param name="deserializable">The stream to deserialize.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The resulting object.</returns>
        Task<TResultType?> Deserialize<TResultType>(Stream deserializable, CancellationToken cancellationToken);
    }
}
