using Amazon.XRay.Recorder.Core.Sampling;

using FluentAssertions;

using NUnit.Framework;

namespace Brighid.Discord.Tracing
{
    public class AwsXRayTracingIdServiceTests
    {
        [TestFixture]
        [Category("Unit")]
        public class CreateIdTests
        {
            [Test, Auto]
            public void ShouldCreateANewTracingId(
                [Target] AwsXRayTracingIdService service
            )
            {
                var result = service.CreateId();

                result.Should().NotBeNullOrEmpty();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class GetIdFromHeaderTests
        {
            [Test, Auto]
            public void ShouldRetrieveIdFromHeaderValue(
                [Target] AwsXRayTracingIdService service
            )
            {
                var id = "1-5759e988-bd862e3fe1be46a994272793";
                var result = service.GetIdFromHeader($"Root={id};Sampled=1");

                result.Should().Be(id);
            }

            [Test, Auto]
            public void ShouldReturnNullIfNoTraceIdWasFound(
                [Target] AwsXRayTracingIdService service
            )
            {
                var result = service.GetIdFromHeader("Sampled=1");

                result.Should().BeNull();
            }
        }
    }
}
