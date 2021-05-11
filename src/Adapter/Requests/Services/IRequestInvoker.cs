using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// Service that invokes requests.
    /// </summary>
    public interface IRequestInvoker
    {
        /// <summary>
        /// Invokes the given <paramref name="request" />.
        /// </summary>
        /// <param name="request">The request to invoke.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Invoke(RequestMessage request, CancellationToken cancellationToken = default);
    }
}
