using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Adapter.Management
{
    /// <summary>
    /// Service used for shifting traffic.
    /// </summary>
    public interface ITrafficShifter
    {
        /// <summary>
        /// Performs a traffic shift.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task PerformTrafficShift(CancellationToken cancellationToken = default);
    }
}
