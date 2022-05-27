using System;
using System.Runtime.CompilerServices;
using System.Threading;

using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using Amazon.XRay.Recorder.Core.Sampling;

namespace Brighid.Discord.Tracing
{
    /// <inheritdoc />
    public class AwsXRayTracingService : ITracingService
    {
        /// <summary>
        /// Service name that shows up in traces.
        /// </summary>
        public const string ServiceName = "Discord";

        private readonly ITracingIdService tracingIdService;
        private readonly IAWSXRayRecorder recorder;
        private readonly AsyncLocal<TraceContext?> context = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AwsXRayTracingService" /> class.
        /// </summary>
        /// <param name="tracingIdService">Service for interacing with trace IDs.</param>
        /// <param name="recorder">The XRay Recorder Client to use.</param>
        public AwsXRayTracingService(
            ITracingIdService tracingIdService,
            IAWSXRayRecorder recorder
        )
        {
            this.tracingIdService = tracingIdService;
            this.recorder = recorder;
        }

        /// <summary>
        /// Gets the current tracing header.
        /// </summary>
        public string? Header => context.Value?.Header;

        /// <inheritdoc />
        public TraceContext StartTrace(string? header = null)
        {
            var sampled = SampleDecision.Sampled;
            var traceId = tracingIdService.GetIdFromHeader(header) ?? tracingIdService.CreateId();
            var traceHeader = new TraceHeader
            {
                RootTraceId = traceId,
                Sampled = sampled,
            };

            context.Value = new TraceContext(this, traceHeader.ToString(), traceHeader.RootTraceId);
            recorder.BeginSegment(
                name: ServiceName,
                traceId: traceId,
                samplingResponse: new SamplingResponse(sampled)
            );

            return context.Value;
        }

        /// <inheritdoc />
        public void EndTrace()
        {
            EnsureActiveTrace();
            recorder.EndSegment();
            context.Value = null;
        }

        /// <inheritdoc />
        public void AddAnnotation(string name, string value)
        {
            EnsureActiveTrace();
            recorder.AddAnnotation(name, value);
        }

        /// <inheritdoc />
        public void AddMetadata(string name, string value)
        {
            EnsureActiveTrace();
            recorder.AddMetadata(name, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureActiveTrace()
        {
            if (context.Value == null)
            {
                throw new InvalidOperationException("A trace must be active in order to use this method.");
            }
        }
    }
}
