using System;
using System.Collections.Concurrent;

using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.Users
{
    /// <inheritdoc />
    public class ConcurrentUserIdCache : ConcurrentDictionary<Snowflake, UserId>, IUserIdCache
    {
        /// <inheritdoc />
        public void RemoveByIdentityId(Guid identityId)
        {
            foreach (var entry in this)
            {
                if (entry.Value.Id == identityId)
                {
                    TryRemove(entry);
                    return;
                }
            }
        }
    }
}
