using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Adapter;
using Brighid.Discord.Adapter.Database;
using Brighid.Discord.Adapter.Messages;
using Brighid.Discord.Adapter.Requests;
using Brighid.Discord.Mocks;
using Brighid.Discord.Models;
using Brighid.Discord.Threading;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

using NSubstitute;

using RichardSzalay.MockHttp;

#pragma warning disable EF1001

internal class AutoAttribute : AutoDataAttribute
{
    public AutoAttribute()
        : base(Create)
    {
    }

    public static IFixture Create()
    {
        var provider = new ServiceCollection()
        .AddLocalization()
        .AddLogging()
        .BuildServiceProvider();

        var fixture = new Fixture();
        var messageHandler = new MockHttpMessageHandler();
        var httpClient = new System.Net.Http.HttpClient(messageHandler) { Timeout = TimeSpan.FromSeconds(1) };
        fixture.Inject(messageHandler);
        fixture.Inject(httpClient);
        fixture.Inject(new CancellationToken(false));
        fixture.Inject<JsonSerializerContext>(JsonContext.Default);
        fixture.Inject(new JwtSecurityTokenHandler { MaximumTokenSizeInBytes = int.MaxValue });
        fixture.Inject(provider.GetRequiredService<IStringLocalizer<Strings>>());
        fixture.Inject(new Endpoint('c', ChannelEndpoint.CreateMessage));
        fixture.Inject(new RequestOptions { BatchingBufferPeriod = 0.05 });
        fixture.Inject<JsonConverter<GatewayMessage>>(new MockGatewayMessageConverter());
        fixture.Inject<IEntityType>(new EntityType("test", new Model(), true, ConfigurationSource.Convention));
        fixture.Register<IChannel<RequestMessage>>(() => new Channel<RequestMessage>());
        fixture.Register(() =>
        {
            var result = Substitute.For<DatabaseContext>();
            result.Database.Returns(Substitute.For<DatabaseFacade>(result));
            return result;
        });
        fixture.Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
        fixture.Customize(new SupportMutableValueTypesCustomization());
        fixture.Customizations.Add(new OptionsRelay());
        fixture.Customizations.Add(new TypeOmitter<IDictionary<string, JsonElement>>());
        fixture.Customizations.Add(new TypeOmitter<JsonConverter>());
        fixture.Customizations.Add(new TypeOmitter<ValueTask<GatewayMessageChunk>>());
        fixture.Customizations.Add(new TypeOmitter<GatewayMessageChunk>());
        fixture.Customizations.Add(new TypeOmitter<Task<GatewayMessage>>());
        fixture.Customizations.Add(new TypeOmitter<GatewayMessage>());
        fixture.Customizations.Add(new TypeOmitter<MemoryStream>());
        fixture.Customizations.Add(new TypeOmitter<ISingletonOptionsInitializer>());
        fixture.Customizations.Insert(-1, new TargetRelay());
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}
