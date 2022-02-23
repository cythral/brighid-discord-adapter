using Amazon.XRay.Recorder.Core.Internal.Entities;

namespace Brighid.Discord.Tracing
{
    /// <inheritdoc />
    public class AwsXRayTracingIdService : ITracingIdService
    {
        /// <inheritdoc/>
        public string CreateId()
        {
            return TraceId.NewId();
        }

        /// <inheritdoc/>
        public string? GetIdFromHeader(string? header)
        {
            return (!TraceHeader.TryParse(header, out var parsedHeader) || string.IsNullOrEmpty(parsedHeader.RootTraceId))
                ? null
                : parsedHeader.RootTraceId;
        }
    }
}
