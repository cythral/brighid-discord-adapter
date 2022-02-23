namespace Brighid.Discord.Tracing
{
    /// <summary>
    /// Service for working with trace IDs.
    /// </summary>
    public interface ITracingIdService
    {
        /// <summary>
        /// Creates a new trace ID.
        /// </summary>
        /// <returns>The resulting trace id.</returns>
        string CreateId();

        /// <summary>
        /// Retrieves a trace id from a header.
        /// </summary>
        /// <param name="header">The header to retrieve a trace ID from.</param>
        /// <returns>The Trace ID from the header, or null if not found.</returns>
        string? GetIdFromHeader(string? header);
    }
}
