using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

#pragma warning disable IDE0050

namespace Brighid.Discord.Serialization
{
    public class JsonSerializerTests
    {
        [Test, Auto]
        public async Task Serialize_SerializesToJson(
            [Target] JsonSerializer serializer
        )
        {
            var serializable = new TestObject { A = "A", B = "B" };
            var cancellationToken = new CancellationToken(false);

            var result = await serializer.Serialize(serializable, cancellationToken);
            result.Should().Be("{\"A\":\"A\",\"B\":\"B\"}");
        }

        [Test, Auto]
        public async Task Deserialize_DeserializesToObject(
            [Target] JsonSerializer serializer
        )
        {
            var deserializable = "{\"A\":\"A\",\"B\":\"B\"}";
            var cancellationToken = new CancellationToken(false);

            var result = await serializer.Deserialize<TestObject>(deserializable, cancellationToken);

            result!.A.Should().Be("A");
            result!.B.Should().Be("B");
        }

        private class TestObject
        {
            public string A { get; set; } = string.Empty;

            public string B { get; set; } = string.Empty;
        }
    }
}
