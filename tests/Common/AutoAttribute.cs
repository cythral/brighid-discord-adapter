using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Messages;
using Brighid.Discord.Adapter.Requests;
using Brighid.Discord.Mocks;
using Brighid.Discord.Threading;

internal class AutoAttribute : AutoDataAttribute
{
    public AutoAttribute()
        : base(Create)
    {
    }

    public static IFixture Create()
    {
        var fixture = new Fixture();
        fixture.Inject(new CancellationToken(false));
        fixture.Inject(new RequestOptions { BatchingBufferPeriod = 0.01 });
        fixture.Inject<JsonConverter<GatewayMessage>>(new MockGatewayMessageConverter());
        fixture.Register<IChannel<RequestMessage>>(() => new Channel<RequestMessage>());
        fixture.Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
        fixture.Customizations.Add(new OptionsRelay());
        fixture.Customizations.Add(new TypeOmitter<IDictionary<string, JsonElement>>());
        fixture.Customizations.Add(new TypeOmitter<JsonConverter>());
        fixture.Customizations.Add(new TypeOmitter<ValueTask<GatewayMessageChunk>>());
        fixture.Customizations.Add(new TypeOmitter<GatewayMessageChunk>());
        fixture.Customizations.Add(new TypeOmitter<Task<GatewayMessage>>());
        fixture.Customizations.Add(new TypeOmitter<GatewayMessage>());
        fixture.Customizations.Add(new TypeOmitter<MemoryStream>());
        fixture.Customizations.Insert(-1, new TargetRelay());
        return fixture;
    }
}
