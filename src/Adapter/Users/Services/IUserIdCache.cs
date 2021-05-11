using System;
using System.Collections.Generic;

using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.Users
{
    /// <summary>
    /// A cache that maps discord snowflake IDs to Identity Server GUIDs.
    /// </summary>
    public interface IUserIdCache : IDictionary<Snowflake, Guid>
    {
    }
}
