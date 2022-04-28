using System;

namespace Brighid.Discord.Adapter.Users
{
    /// <summary>
    /// A user's id.
    /// </summary>
    public struct UserId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserId" /> struct.
        /// </summary>
        /// <param name="id">The user id.</param>
        /// <param name="debug">A value indicating whether or not the user has debug mode enabled.</param>
        /// <param name="enabled">A value indicating whether or not the user ID is enabled.</param>
        public UserId(
            Guid id,
            bool debug,
            bool enabled
        )
        {
            Id = id;
            Debug = debug;
            Enabled = enabled;
        }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Gets a value indicating whether debug mode is enabled for this user.
        /// </summary>
        public bool Debug { get; init; }

        /// <summary>
        /// Gets a value indicating whether the user is enabled or not.
        /// </summary>
        public bool Enabled { get; init; }
    }
}
