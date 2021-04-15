using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Messages;
using Brighid.Discord.Mocks;

internal class AutoAttribute : AutoDataAttribute
{
    public AutoAttribute()
        : base(Create)
    {
    }

    public static IFixture Create()
    {
        var fixture = new Fixture();
        fixture.Inject<JsonConverter<GatewayMessage>>(new MockGatewayMessageConverter());
        fixture.Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
        fixture.Customizations.Add(new OptionsRelay());
        fixture.Customizations.Add(new TypeOmitter<IDictionary<string, JsonElement>>());
        fixture.Customizations.Add(new TypeOmitter<JsonConverter<GatewayMessage>>());
        fixture.Customizations.Insert(-1, new TargetRelay());
        return fixture;
    }
}
