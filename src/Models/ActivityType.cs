namespace Brighid.Discord.Models
{
    /// <summary>
    /// An Activity's Type.
    /// </summary>
    public enum ActivityType
    {
        /// <summary>
        /// Game Activity.
        /// </summary>
        Game = 0,

        /// <summary>
        /// Streaming Activity.
        /// </summary>
        Streaming = 1,

        /// <summary>
        /// Listening Activity.
        /// </summary>
        Listening = 2,

        /// <summary>
        /// Watching Activity.
        /// </summary>
        Watching = 3,

        /// <summary>
        /// Custom Activity.
        /// </summary>
        Custom = 4,

        /// <summary>
        /// Competing Activity.
        /// </summary>
        Competing = 5,
    }
}
