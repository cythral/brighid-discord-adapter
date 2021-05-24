using System.Threading.Tasks;

namespace Brighid.Discord.RestClient.Responses
{
    /// <summary>
    /// TCP Listener / Server that accepts incoming TCP connections and listens for messages.
    /// </summary>
    public interface ITcpListener
    {
        /// <summary>
        /// Start accepting connections.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop accepting connections.
        /// </summary>
        void Stop();

        /// <summary>
        /// Accept a new client connection.
        /// </summary>
        /// <returns>The resulting client.</returns>
        Task<ITcpClient> Accept();
    }
}
