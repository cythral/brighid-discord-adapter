using System;

namespace Brighid.Discord.Tracing
{
    /// <summary>
    /// Context for tracing information.
    /// </summary>
    public class TraceContext : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceContext" /> class.
        /// </summary>
        /// <param name="service">Service that created this context.</param>
        /// <param name="header">The tracing header to use to track a request.</param>
        public TraceContext(
            ITracingService service,
            string header
        )
        {
            Service = service;
            Header = header;
        }

        /// <summary>
        /// Gets or sets the tracing service associated with this context.
        /// </summary>
        public ITracingService Service { get; set; }

        /// <summary>
        /// Gets or sets the trace header.
        /// </summary>
        public string Header { get; set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Service.EndTrace();
            GC.SuppressFinalize(this);
        }
    }
}
