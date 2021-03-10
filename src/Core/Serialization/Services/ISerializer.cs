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
        /// <returns>The created task.</returns>
        Task<string> Serialize<TSerializableType>(TSerializableType serializable, CancellationToken cancellationToken);
    }
}
