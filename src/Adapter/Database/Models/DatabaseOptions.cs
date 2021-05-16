namespace Brighid.Discord.Adapter.Database
{
    /// <summary>
    /// Options to use when interacting with the database.
    /// </summary>
    public class DatabaseOptions
    {
        /// <summary>
        /// Gets or sets the database host name.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the database to use.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the database user to use.
        /// </summary>
        public string User { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the database password to use.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
