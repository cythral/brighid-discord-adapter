using System;

namespace Brighid.Discord.Adapter
{
    /// <summary>
    /// Represents flags for a user from the Identity Service.
    /// </summary>
    [Flags]
    public enum UserFlags : long
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Flag indicating a user has debug mode enabled.
        /// </summary>
        Debug = 1,
    }
}
