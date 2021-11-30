using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Models;
using Brighid.Discord.RestClient.Responses;
using Brighid.Discord.Serialization;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.RestClient.Client
{
    public class DefaultDiscordRequestHandlerTests
    {
        [TestFixture]
        public class HandleTests
        {
            [Test, Auto]
            public async Task ShouldQueueARequestWithNoResponseURL(
                Snowflake recipientId,
                Endpoint endpoint,
                object request,
                Dictionary<string, string> parameters,
                Dictionary<string, string> headers,
                [Frozen, Substitute] IResponseService server,
                [Frozen, Substitute] IRequestQueuer queuer,
                [Target] DefaultDiscordRequestHandler handler,
                CancellationToken cancellationToken
            )
            {
                await handler.Handle(endpoint, request, parameters, headers, cancellationToken);

                await queuer.Received().QueueRequest(Is<Request>(req => req.ResponseURL == null), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldQueueARequestWithTheCorrectEndpoint(
                Snowflake recipientId,
                Endpoint endpoint,
                object request,
                Dictionary<string, string> parameters,
                Dictionary<string, string> headers,
                [Frozen, Substitute] IResponseService server,
                [Frozen, Substitute] IRequestQueuer queuer,
                [Target] DefaultDiscordRequestHandler handler,
                CancellationToken cancellationToken
            )
            {
                await handler.Handle(endpoint, request, parameters, headers, cancellationToken);

                await queuer.Received().QueueRequest(Any<Request>(), Is(cancellationToken));
                var receivedRequest = TestUtils.GetArg<Request>(queuer, nameof(IRequestQueuer.QueueRequest), 0);
                receivedRequest.Endpoint.Should().Be(endpoint);
            }

            [Test, Auto]
            public async Task ShouldQueueARequestWithTheParameters(
                Snowflake recipientId,
                Endpoint endpoint,
                object request,
                Dictionary<string, string> parameters,
                Dictionary<string, string> headers,
                [Frozen, Substitute] IResponseService server,
                [Frozen, Substitute] IRequestQueuer queuer,
                [Target] DefaultDiscordRequestHandler handler,
                CancellationToken cancellationToken
            )
            {
                await handler.Handle(endpoint, request, parameters, headers, cancellationToken);

                await queuer.Received().QueueRequest(Any<Request>(), Is(cancellationToken));
                var receivedRequest = TestUtils.GetArg<Request>(queuer, nameof(IRequestQueuer.QueueRequest), 0);
                receivedRequest.Parameters.Should().BeEquivalentTo(parameters);
            }

            [Test, Auto]
            public async Task ShouldQueueARequestWithTheHeaders(
                Snowflake recipientId,
                Endpoint endpoint,
                object request,
                Dictionary<string, string> parameters,
                Dictionary<string, string> headers,
                [Frozen, Substitute] IResponseService server,
                [Frozen, Substitute] IRequestQueuer queuer,
                [Target] DefaultDiscordRequestHandler handler,
                CancellationToken cancellationToken
            )
            {
                await handler.Handle(endpoint, request, parameters, headers, cancellationToken);

                await queuer.Received().QueueRequest(Any<Request>(), Is(cancellationToken));
                var receivedRequest = TestUtils.GetArg<Request>(queuer, nameof(IRequestQueuer.QueueRequest), 0);
                receivedRequest.Headers.Should().BeEquivalentTo(headers);
            }

            [Test, Auto]
            public async Task ShouldQueueARequestWithTheSerializedBody(
                Snowflake recipientId,
                Endpoint endpoint,
                object request,
                string body,
                Dictionary<string, string> parameters,
                Dictionary<string, string> headers,
                [Frozen, Substitute] ISerializer serializer,
                [Frozen, Substitute] IResponseService server,
                [Frozen, Substitute] IRequestQueuer queuer,
                [Target] DefaultDiscordRequestHandler handler,
                CancellationToken cancellationToken
            )
            {
                serializer.Serialize(Any<object>()).Returns(body);
                await handler.Handle(endpoint, request, parameters, headers, cancellationToken);

                await queuer.Received().QueueRequest(Any<Request>(), Is(cancellationToken));
                var receivedRequest = TestUtils.GetArg<Request>(queuer, nameof(IRequestQueuer.QueueRequest), 0);
                receivedRequest.RequestBody.Should().Be(body);
            }
        }

        [TestFixture]
        public class HandleAndWaitTests
        {
            [Test, Auto]
            public async Task ShouldQueueARequestWithTheCorrectResponseURL(
                Snowflake recipientId,
                Endpoint endpoint,
                object request,
                Dictionary<string, string> parameters,
                Dictionary<string, string> headers,
                [Frozen, Substitute] IResponseService server,
                [Frozen, Substitute] IRequestQueuer queuer,
                [Target] DefaultDiscordRequestHandler handler,
                CancellationToken cancellationToken
            )
            {
                await handler.Handle<object, object>(endpoint, request, parameters, headers, cancellationToken);

                await queuer.Received().QueueRequest(Is<Request>(req => req.ResponseURL == server.Uri), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldQueueARequestWithTheCorrectEndpoint(
                Snowflake recipientId,
                Endpoint endpoint,
                object request,
                Dictionary<string, string> parameters,
                Dictionary<string, string> headers,
                [Frozen, Substitute] IResponseService server,
                [Frozen, Substitute] IRequestQueuer queuer,
                [Target] DefaultDiscordRequestHandler handler,
                CancellationToken cancellationToken
            )
            {
                await handler.Handle<object, object>(endpoint, request, parameters, headers, cancellationToken);

                await queuer.Received().QueueRequest(Any<Request>(), Is(cancellationToken));
                var receivedRequest = TestUtils.GetArg<Request>(queuer, nameof(IRequestQueuer.QueueRequest), 0);
                receivedRequest.Endpoint.Should().Be(endpoint);
            }

            [Test, Auto]
            public async Task ShouldQueueARequestWithTheParameters(
                Snowflake recipientId,
                Endpoint endpoint,
                object request,
                Dictionary<string, string> parameters,
                Dictionary<string, string> headers,
                [Frozen, Substitute] IResponseService server,
                [Frozen, Substitute] IRequestQueuer queuer,
                [Target] DefaultDiscordRequestHandler handler,
                CancellationToken cancellationToken
            )
            {
                await handler.Handle<object, object>(endpoint, request, parameters, headers, cancellationToken);

                await queuer.Received().QueueRequest(Any<Request>(), Is(cancellationToken));
                var receivedRequest = TestUtils.GetArg<Request>(queuer, nameof(IRequestQueuer.QueueRequest), 0);
                receivedRequest.Parameters.Should().BeEquivalentTo(parameters);
            }

            [Test, Auto]
            public async Task ShouldQueueARequestWithTheHeaders(
                Snowflake recipientId,
                Endpoint endpoint,
                object request,
                Dictionary<string, string> parameters,
                Dictionary<string, string> headers,
                [Frozen, Substitute] IResponseService server,
                [Frozen, Substitute] IRequestQueuer queuer,
                [Target] DefaultDiscordRequestHandler handler,
                CancellationToken cancellationToken
            )
            {
                await handler.Handle<object, object>(endpoint, request, parameters, headers, cancellationToken);

                await queuer.Received().QueueRequest(Any<Request>(), Is(cancellationToken));
                var receivedRequest = TestUtils.GetArg<Request>(queuer, nameof(IRequestQueuer.QueueRequest), 0);
                receivedRequest.Headers.Should().BeEquivalentTo(headers);
            }

            [Test, Auto]
            public async Task ShouldQueueARequestWithTheSerializedBody(
                Snowflake recipientId,
                Endpoint endpoint,
                object request,
                string body,
                Dictionary<string, string> parameters,
                Dictionary<string, string> headers,
                [Frozen, Substitute] ISerializer serializer,
                [Frozen, Substitute] IResponseService server,
                [Frozen, Substitute] IRequestQueuer queuer,
                [Target] DefaultDiscordRequestHandler handler,
                CancellationToken cancellationToken
            )
            {
                serializer.Serialize(Any<object>()).Returns(body);
                await handler.Handle<object, object>(endpoint, request, parameters, headers, cancellationToken);

                await queuer.Received().QueueRequest(Any<Request>(), Is(cancellationToken));
                var receivedRequest = TestUtils.GetArg<Request>(queuer, nameof(IRequestQueuer.QueueRequest), 0);
                receivedRequest.RequestBody.Should().Be(body);
            }

            [Test, Auto]
            public async Task ShouldListenForAResponse(
                Snowflake recipientId,
                Endpoint endpoint,
                object request,
                Dictionary<string, string> parameters,
                Dictionary<string, string> headers,
                [Frozen, Substitute] IResponseService server,
                [Target] DefaultDiscordRequestHandler handler,
                CancellationToken cancellationToken
            )
            {
                await handler.Handle<object, object>(endpoint, request, parameters, headers, cancellationToken);

                await server.Received().ListenForResponse(Any<Guid>(), Any<TaskCompletionSource<Response>>());
            }

            [Test, Auto]
            public async Task ShouldDeserializeAndReturnResponse(
                Snowflake recipientId,
                Endpoint endpoint,
                object request,
                object deserializedResponse,
                Dictionary<string, string> parameters,
                Dictionary<string, string> headers,
                [Frozen] Response response,
                [Frozen, Substitute] ISerializer serializer,
                [Frozen, Substitute] IResponseService server,
                [Target] DefaultDiscordRequestHandler handler,
                CancellationToken cancellationToken
            )
            {
                serializer.Deserialize<object>(Any<string>()).Returns(response);
                var result = await handler.Handle<object, object>(endpoint, request, parameters, headers, cancellationToken);

                result.Should().Be(response);
                serializer.Received().Deserialize<object>(Is(response.Body!));
            }
        }
    }
}
