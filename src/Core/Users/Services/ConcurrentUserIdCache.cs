using System;
using System.Collections.Concurrent;

using Brighid.Discord.Models;

namespace Brighid.Discord.Users
{
    /// <inheritdoc />
    public class ConcurrentUserIdCache : ConcurrentDictionary<Snowflake, Guid>, IUserIdCache
    {
    }
}
