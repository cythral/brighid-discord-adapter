using System;

using AutoFixture.NUnit3;

using FluentAssertions;

using NUnit.Framework;

namespace Brighid.Discord.Adapter.Management
{
    [TestFixture]
    [Category("Unit")]
    public class DiscordApiInfoServiceTests
    {
        [Test, Auto]
        public void ClientIdShouldReturnClientId(
            string clientId,
            [Frozen] AdapterOptions options,
            [Target] DiscordApiInfoService service
        )
        {
            options.ClientId = clientId;

            service.ClientId.Should().Be(clientId);
        }

        [Test, Auto]
        public void ClientSecretShouldReturnClientSecret(
            string clientSecret,
            [Frozen] AdapterOptions options,
            [Target] DiscordApiInfoService service
        )
        {
            options.ClientSecret = clientSecret;

            service.ClientSecret.Should().Be(clientSecret);
        }

        [Test, Auto]
        public void ApiBaseUrlShouldReturnBaseUrlWithVersion(
            string apiVersion,
            [Frozen] AdapterOptions options,
            [Target] DiscordApiInfoService service
        )
        {
            options.DiscordApiBaseUrl = new Uri("https://discord.com/api");
            options.ApiVersion = apiVersion;

            service.ApiBaseUrl.Should().Be($"https://discord.com/api/{apiVersion}");
        }

        [Test, Auto]
        public void OAuth2TokenEndpointShouldReturnFullyQualifiedTokenEndpoint(
            string apiVersion,
            [Frozen] AdapterOptions options,
            [Target] DiscordApiInfoService service
        )
        {
            options.DiscordApiBaseUrl = new Uri("https://discord.com/api");
            options.ApiVersion = apiVersion;

            service.OAuth2TokenEndpoint.Should().Be($"https://discord.com/api/{apiVersion}/oauth2/token");
        }

        [Test, Auto]
        public void OAuth2UserInfoEndpointShouldReturnFullyQualifiedUserInfoEndpoint(
            string apiVersion,
            [Frozen] AdapterOptions options,
            [Target] DiscordApiInfoService service
        )
        {
            options.DiscordApiBaseUrl = new Uri("https://discord.com/api");
            options.ApiVersion = apiVersion;

            service.OAuth2UserInfoEndpoint.Should().Be($"https://discord.com/api/{apiVersion}/users/@me");
        }

        [Test, Auto]
        public void Oauth2RedirectUriShouldReturnOauth2RedirectUri(
            [Frozen] AdapterOptions options,
            [Target] DiscordApiInfoService service
        )
        {
            service.OAuth2RedirectUri.Should().Be(options.OAuth2RedirectUri);
        }
    }
}
