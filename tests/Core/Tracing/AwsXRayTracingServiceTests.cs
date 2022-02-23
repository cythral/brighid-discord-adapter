using System;

using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Sampling;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Tracing
{
    public class AwsXRayTracingServiceTests
    {
        [TestFixture]
        [Category("Unit")]
        public class HeaderTests
        {
            [Test, Auto]
            public void ShouldReturnTheCurrentTraceHeader(
                string id,
                [Frozen, Substitute] ITracingIdService tracingIdService,
                [Target] AwsXRayTracingService service
            )
            {
                tracingIdService.CreateId().Returns(id);
                tracingIdService.GetIdFromHeader(Any<string?>()).Returns(null as string);

                service.StartTrace();
                service.Header.Should().Be($"Root={id}; Sampled=1");
            }

            [Test, Auto]
            public void ShouldReturnNullIfNoTraceIsActive(
                string id,
                string parent,
                string header,
                [Frozen, Substitute] ITracingIdService tracingIdService,
                [Target] AwsXRayTracingService service
            )
            {
                service.Header.Should().BeNull();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class StartTraceTests
        {
            [Test, Auto]
            public void ShouldStartSegmentWithNewId(
                string traceId,
                [Frozen, Substitute] ITracingIdService tracingIdService,
                [Frozen, Substitute] IAWSXRayRecorder recorder,
                [Target] AwsXRayTracingService service
            )
            {
                tracingIdService.CreateId().Returns(traceId);
                tracingIdService.GetIdFromHeader(Any<string>()).Returns(null as string);

                service.StartTrace();

                recorder.Received().BeginSegment(
                    name: Is(AwsXRayTracingService.ServiceName),
                    traceId: Is(traceId),
                    parentId: Is(null as string),
                    samplingResponse: Is<SamplingResponse>(response => response.SampleDecision == SampleDecision.Sampled)
                );
            }

            [Test, Auto]
            public void ShouldStopTraceWhenContextIsDisposed(
                string header,
                string parentId,
                string traceId,
                [Frozen, Substitute] ITracingIdService tracingIdService,
                [Frozen, Substitute] IAWSXRayRecorder recorder,
                [Target] AwsXRayTracingService service
            )
            {
                tracingIdService.CreateId().Returns(traceId);
                tracingIdService.GetIdFromHeader(Is(header)).Returns(parentId);

                using (var context = service.StartTrace(header))
                {
                }

                recorder.Received().EndSegment();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class EndTraceTests
        {
            [Test, Auto]
            public void ShouldEndTraceSegments(
                [Frozen] IAWSXRayRecorder recorder,
                [Target] AwsXRayTracingService service
            )
            {
                service.StartTrace();
                service.EndTrace();
                recorder.Received().EndSegment();
            }

            [Test, Auto]
            public void ShouldThrowInvalidOperationExceptionIfNoTraceIsActive(
                [Frozen] IAWSXRayRecorder recorder,
                [Target] AwsXRayTracingService service
            )
            {
                Action func = service.EndTrace;

                func.Should().Throw<InvalidOperationException>();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class AddAnnotationTests
        {
            [Test, Auto]
            public void ShouldAddAnAnnotation(
                string name,
                string value,
                [Frozen] IAWSXRayRecorder recorder,
                [Target] AwsXRayTracingService service
            )
            {
                service.StartTrace();
                service.AddAnnotation(name, value);

                recorder.Received().AddAnnotation(Is(name), Is(value));
            }

            [Test, Auto]
            public void ShouldThrowIfNoTraceIsActive(
                string name,
                string value,
                [Frozen] IAWSXRayRecorder recorder,
                [Target] AwsXRayTracingService service
            )
            {
                Action func = () => service.AddAnnotation(name, value);

                func.Should().Throw<InvalidOperationException>();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class AddMetadataTests
        {
            [Test, Auto]
            public void ShouldAddMetadata(
                string name,
                string value,
                [Frozen] IAWSXRayRecorder recorder,
                [Target] AwsXRayTracingService service
            )
            {
                service.StartTrace();
                service.AddMetadata(name, value);

                recorder.Received().AddMetadata(Is(name), Is(value));
            }

            [Test, Auto]
            public void ShouldThrowIfNoTraceIsActive(
                string name,
                string value,
                [Frozen] IAWSXRayRecorder recorder,
                [Target] AwsXRayTracingService service
            )
            {
                Action func = () => service.AddMetadata(name, value);

                func.Should().Throw<InvalidOperationException>();
            }
        }
    }
}
