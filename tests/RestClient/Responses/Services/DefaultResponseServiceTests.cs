using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Models;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.RestClient.Responses
{
    public class DefaultResponseServiceTests
    {
        [TestFixture]
        [Category("Unit")]
        public class HandleResponseTests
        {
            [Test, Auto, Timeout(500)]
            public async Task ShouldResolveAssociatedPromiseIfItExistsInRequestMap(
                Response response,
                [Frozen, Substitute] IRequestMap requestMap,
                [Target] DefaultResponseService service
            )
            {
                var promise = new TaskCompletionSource<Response>();

                requestMap.TryGetValue(Any<Guid>(), out Any<TaskCompletionSource<Response>>()!).Returns(x =>
                {
                    x[1] = promise;
                    return true;
                });

                service.HandleResponse(response);

                var result = await promise.Task;
                result.Should().Be(response);

                requestMap.Received().TryGetValue(Is(response.RequestId), out Any<TaskCompletionSource<Response>>()!);
            }

            [Test, Auto, Timeout(500)]
            public async Task ShouldRemovePromiseFromMap(
                Response response,
                [Frozen, Substitute] IRequestMap requestMap,
                [Target] DefaultResponseService service
            )
            {
                var promise = new TaskCompletionSource<Response>();

                requestMap.TryGetValue(Any<Guid>(), out Any<TaskCompletionSource<Response>>()!).Returns(x =>
                {
                    x[1] = promise;
                    return true;
                });

                service.HandleResponse(response);

                var result = await promise.Task;
                result.Should().Be(response);

                requestMap.Received().Remove(Is(response.RequestId));
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class ListenForResponseTests
        {
            [Test, Auto]
            public async Task ShouldRemovePromiseFromMapWhenCanceled(
                Response response,
                [Frozen, Substitute] IRequestMap requestMap,
                [Target] DefaultResponseService service
            )
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;
                var promise = new TaskCompletionSource<Response>();

                var responseTask = service.ListenForResponse(response.RequestId, promise, cancellationToken);
                var func = async () => await responseTask;

                var expectation = func.Should().ThrowAsync<OperationCanceledException>();
                cancellationTokenSource.CancelAfter(10);

                await expectation;
                requestMap.Received().Remove(Is(response.RequestId));
            }
        }
    }
}
