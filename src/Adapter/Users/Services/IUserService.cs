using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.Users
{
    /// <summary>
    /// Service for managing users.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Determines if a user is registered or not.
        /// </summary>
        /// <param name="user">The user to check registration for.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>True if the user is registered, or false if not.</returns>
        Task<bool> IsUserRegistered(User user, CancellationToken cancellationToken = default);
    }
}
