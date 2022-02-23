namespace Brighid.Discord.Tracing
{
    /// <summary>
    /// Service for performing traces.
    /// </summary>
    public interface ITracingService
    {
        /// <summary>
        /// Gets the current tracing header.
        /// </summary>
        string? Header { get; }

        /// <summary>
        /// Starts or continues a trace.  A trace is continued if <paramref name="header" /> is given, otherwise a new one is started..
        /// </summary>
        /// <param name="header">Header to continue a trace from.</param>
        /// <returns>The tracing context.</returns>
        TraceContext StartTrace(string? header = null);

        /// <summary>
        /// Ends the current trace.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when no trace is active.</exception>
        void EndTrace();

        /// <summary>
        /// Adds an annotation to the current trace.  Annotations differ from metadata in that they can be indexed and sorted.
        /// </summary>
        /// <param name="name">The name of the annotation to add to the trace.</param>
        /// <param name="value">The value of the annotation to add to the trace.</param>
        /// <exception cref="System.InvalidOperationException">Thrown when no trace is active.</exception>
        void AddAnnotation(string name, string value);

        /// <summary>
        /// Adds metadata to the current trace.  Metadata is not indexable like annotations are.
        /// </summary>
        /// <param name="name">The name of the metadata to add to the trace.</param>
        /// <param name="value">The value of the metadata to add to the trace.</param>
        /// <exception cref="System.InvalidOperationException">Thrown when no trace is active.</exception>
        void AddMetadata(string name, string value);
    }
}
