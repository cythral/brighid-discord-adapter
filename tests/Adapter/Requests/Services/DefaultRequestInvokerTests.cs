using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Models;

using FluentAssertions;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Requests
{
    public class DefaultRequestInvokerTests
    {
        [TestFixture]
        public class InvokeTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                RequestMessage request,
                [Target] DefaultRequestInvoker invoker
            )
            {
                var cancellationToken = new CancellationToken(true);
                Func<Task> func = () => invoker.Invoke(request, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCanceled(
                RequestMessage request,
                [Target] DefaultRequestInvoker invoker,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => invoker.Invoke(request, cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldSendRequestWithCorrectMethod(
                RequestMessage request,
                [Frozen, Substitute] HttpMessageInvoker client,
                [Target] DefaultRequestInvoker invoker,
                CancellationToken cancellationToken
            )
            {
                await invoker.Invoke(request, cancellationToken);

                await client.Received().SendAsync(Any<HttpRequestMessage>(), Is(cancellationToken));
                var httpRequest = GetArg<HttpRequestMessage>(client, nameof(HttpMessageInvoker.SendAsync), 0);

                httpRequest.Method.Should().Be(request.RequestDetails.Endpoint.GetMethod());
            }

            [Test, Auto]
            public async Task ShouldSendRequestWithBuiltUrl(
                Uri url,
                RequestMessage request,
                [Frozen, Substitute] HttpMessageInvoker client,
                [Frozen, Substitute] IUrlBuilder urlBuilder,
                [Target] DefaultRequestInvoker invoker,
                CancellationToken cancellationToken
            )
            {
                urlBuilder.BuildFromRequest(Any<Request>()).Returns(url);
                await invoker.Invoke(request, cancellationToken);

                urlBuilder.Received().BuildFromRequest(Is(request.RequestDetails));
                await client.Received().SendAsync(Any<HttpRequestMessage>(), Is(cancellationToken));
                var httpRequest = GetArg<HttpRequestMessage>(client, nameof(HttpMessageInvoker.SendAsync), 0);

                httpRequest.RequestUri.Should().Be(url);
            }

            [Test, Auto]
            public async Task ShouldSendRequestWithGivenBody(
                RequestMessage request,
                [Frozen, Substitute] HttpMessageInvoker client,
                [Frozen, Substitute] IUrlBuilder urlBuilder,
                [Target] DefaultRequestInvoker invoker,
                CancellationToken cancellationToken
            )
            {
                await invoker.Invoke(request, cancellationToken);

                await client.Received().SendAsync(Any<HttpRequestMessage>(), Is(cancellationToken));
                var httpRequest = GetArg<HttpRequestMessage>(client, nameof(HttpMessageInvoker.SendAsync), 0);

                var content = httpRequest.Content.Should().BeOfType<StringContent>().Which;
                var contentString = await content.ReadAsStringAsync(cancellationToken);

                contentString.Should().Be(request.RequestDetails.RequestBody);
                content.Headers.ContentType!.MediaType.Should().BeEquivalentTo("application/json");
            }

            [Test, Auto]
            public async Task ShouldSendRequestWithNoBody(
                RequestMessage request,
                [Frozen, Substitute] HttpMessageInvoker client,
                [Frozen, Substitute] IUrlBuilder urlBuilder,
                [Target] DefaultRequestInvoker invoker,
                CancellationToken cancellationToken
            )
            {
                request.RequestDetails = new Request(Endpoint.ChannelCreateMessage) { RequestBody = null };
                await invoker.Invoke(request, cancellationToken);

                await client.Received().SendAsync(Any<HttpRequestMessage>(), Is(cancellationToken));
                var httpRequest = GetArg<HttpRequestMessage>(client, nameof(HttpMessageInvoker.SendAsync), 0);

                httpRequest.Content.Should().BeNull();
            }

            [Test, Auto]
            public async Task ShouldCompleteRequest(
                string response,
                RequestMessage request,
                [Frozen] HttpStatusCode statusCode,
                [Frozen] HttpResponseMessage httpResponse,
                [Frozen, Substitute] HttpMessageInvoker client,
                [Frozen, Substitute] IRequestMessageRelay relay,
                [Target] DefaultRequestInvoker invoker,
                CancellationToken cancellationToken
            )
            {
                httpResponse.Content = new StringContent(response);
                request.RequestDetails = new Request(Endpoint.ChannelCreateMessage) { RequestBody = null };
                await invoker.Invoke(request, cancellationToken);

                await client.Received().SendAsync(Any<HttpRequestMessage>(), Is(cancellationToken));
                await relay.Received().Complete(Is(request), Is(statusCode), Is(response), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldFailRequestIfExceptionWasThrown(
                string response,
                RequestMessage request,
                [Frozen] HttpStatusCode statusCode,
                [Frozen] HttpResponseMessage httpResponse,
                [Frozen, Substitute] HttpMessageInvoker client,
                [Frozen, Substitute] IRequestMessageRelay relay,
                [Target] DefaultRequestInvoker invoker,
                CancellationToken cancellationToken
            )
            {
                client.SendAsync(Any<HttpRequestMessage>(), Any<CancellationToken>()).Throws<Exception>();
                request.RequestDetails = new Request(Endpoint.ChannelCreateMessage) { RequestBody = null };

                await invoker.Invoke(request, cancellationToken);

                await client.Received().SendAsync(Any<HttpRequestMessage>(), Is(cancellationToken));
                await relay.Received().Fail(Is(request), Is(0U), Is(cancellationToken));
            }

            private static TArg GetArg<TArg>(object target, string methodName, int arg)
            {
                return (from call in target.ReceivedCalls()
                        let methodInfo = call.GetMethodInfo()
                        where methodInfo.Name == methodName
                        select (TArg)call.GetArguments()[arg]).First();
            }
        }
    }
}
