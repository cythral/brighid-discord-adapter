using System;

using Brighid.Discord.Models;

using FluentAssertions;

using NUnit.Framework;

namespace Brighid.Discord.Adapter.Requests
{
    public class BucketTests
    {
        [TestFixture]
        public class AddEndpointTests
        {
            [Test, Auto]
            public void ShouldAddEndpointToTheBucket()
            {
                var bucket = new Bucket
                {
                    ApiCategory = 'c',
                };

                bucket.AddEndpoint(ChannelEndpoint.CreateMessage);
                bucket.HasEndpoint(ChannelEndpoint.CreateMessage).Should().BeTrue();
            }

            [Test, Auto]
            public void ShouldAddMultipleEndpointsToTheBucket()
            {
                var bucket = new Bucket
                {
                    ApiCategory = 'c',
                };

                bucket.AddEndpoint(ChannelEndpoint.CreateMessage);
                bucket.AddEndpoint(ChannelEndpoint.DeleteMessage);
                bucket.HasEndpoint(ChannelEndpoint.CreateMessage).Should().BeTrue();
                bucket.HasEndpoint(ChannelEndpoint.DeleteMessage).Should().BeTrue();
            }

            [Test, Auto]
            public void ShouldThrowIfCategoriesDontMatch()
            {
                var bucket = new Bucket
                {
                    ApiCategory = 'd',
                };

                Action func = () => bucket.AddEndpoint(ChannelEndpoint.CreateMessage);

                func.Should().Throw<ArgumentException>();
            }
        }

        [TestFixture]
        public class RemoveEndpointTests
        {
            [Test, Auto]
            public void ShouldRemoveEndpointsFromTheBucket()
            {
                var bucket = new Bucket
                {
                    ApiCategory = 'c',
                    Endpoints = (ulong)(ChannelEndpoint.CreateMessage | ChannelEndpoint.DeleteMessage),
                };

                bucket.RemoveEndpoint(ChannelEndpoint.CreateMessage);
                bucket.HasEndpoint(ChannelEndpoint.CreateMessage).Should().BeFalse();
                bucket.HasEndpoint(ChannelEndpoint.DeleteMessage).Should().BeTrue();
            }

            [Test, Auto]
            public void ShouldThrowIfCategoriesDontMatch()
            {
                var bucket = new Bucket
                {
                    ApiCategory = 'd',
                };

                Action func = () => bucket.RemoveEndpoint(ChannelEndpoint.CreateMessage);

                func.Should().Throw<ArgumentException>();
            }
        }
    }
}
