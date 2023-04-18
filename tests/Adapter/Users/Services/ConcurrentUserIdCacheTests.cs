using Brighid.Discord.Models;

using FluentAssertions;

using NUnit.Framework;

namespace Brighid.Discord.Adapter.Users
{
    public class ConcurrentUserIdCacheTests
    {
        [TestFixture]
        [Category("Unit")]
        public class RemoveByIdentityId
        {
            [Test, Auto]
            public void ShouldRemoveTheCacheEntryForTheGivenIdentityId(
                Snowflake discordId1,
                Snowflake discordId2,
                UserId identityId1,
                UserId identityId2,
                [Target] ConcurrentUserIdCache cache
            )
            {
                cache[discordId1] = identityId1;
                cache[discordId2] = identityId2;

                cache.RemoveByIdentityId(identityId1.Id);

                cache.Should().NotContainKey(discordId1);
                cache.Should().ContainKey(discordId2).WhoseValue.Should().Be(identityId2);
            }
        }
    }
}
