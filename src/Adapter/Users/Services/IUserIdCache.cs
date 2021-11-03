using System;
using System.Collections.Generic;

using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.Users
{
    /// <summary>
    /// A cache that maps discord snowflake IDs to Identity Server GUIDs.
    /// </summary>
    public interface IUserIdCache : IDictionary<Snowflake, UserId>
    {
        /// <summary>
        /// Remove a cache entry by the <paramref name="identityId" />.
        /// </summary>
        /// <param name="identityId">ID of the user to remove from the cache.</param>
        void RemoveByIdentityId(Guid identityId);
    }
}
