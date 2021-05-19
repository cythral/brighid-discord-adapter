using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.SNSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;

using AutoFixture.NUnit3;

using Brighid.Discord.Models;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.ResponseHandler
{
    public class HandlerTests
    {
        [Test, Auto]
        public async Task ShouldThrowIfCancelled(
            SNSEvent snsEvent,
            [Target] Handler handler
        )
        {
            var cancellationToken = new CancellationToken(true);
            Func<Task> func = () => handler.Handle(snsEvent, cancellationToken);

            await func.Should().ThrowAsync<OperationCanceledException>();
        }

        [Test, Auto]
        public async Task ShouldNotThrowIfNotCancelled(
            SNSEvent snsEvent,
            [Target] Handler handler,
            CancellationToken cancellationToken
        )
        {
            Func<Task> func = () => handler.Handle(snsEvent, cancellationToken);

            await func.Should().NotThrowAsync<OperationCanceledException>();
        }

        [Test, Auto]
        public async Task ShouldPublishMessagesWithBodyToSQS(
            SNSEvent snsEvent,
            SNSEvent.SNSRecord record1,
            SNSEvent.SNSRecord record2,
            Request request1,
            Request request2,
            [Frozen] IAmazonSQS sqs,
            [Frozen] ISnsRecordMapper mapper,
            [Target] Handler handler,
            CancellationToken cancellationToken
        )
        {
            snsEvent.Records = new List<SNSEvent.SNSRecord> { record1, record2 };

            mapper.MapToRequest(Is(record1)).Returns(request1);
            mapper.MapToRequest(Is(record2)).Returns(request2);
            var serializedRequest1 = JsonSerializer.Serialize(request1);
            var serializedRequest2 = JsonSerializer.Serialize(request2);

            await handler.Handle(snsEvent, cancellationToken);

            await sqs.Received().SendMessageBatchAsync(Any<SendMessageBatchRequest>(), Is(cancellationToken));
            var request = TestUtils.GetArg<SendMessageBatchRequest>(sqs, nameof(IAmazonSQS.SendMessageBatchAsync), 0);
            request.Entries.Should().Contain(entry => entry.MessageBody == serializedRequest1);
            request.Entries.Should().Contain(entry => entry.MessageBody == serializedRequest2);
        }

        [Test, Auto]
        public async Task ShouldPublishMessagesWithIdsToSQS(
            SNSEvent snsEvent,
            SNSEvent.SNSRecord record1,
            SNSEvent.SNSRecord record2,
            Request request1,
            Request request2,
            [Frozen] IAmazonSQS sqs,
            [Frozen] ISnsRecordMapper mapper,
            [Target] Handler handler,
            CancellationToken cancellationToken
        )
        {
            snsEvent.Records = new List<SNSEvent.SNSRecord> { record1, record2 };

            mapper.MapToRequest(Is(record1)).Returns(request1);
            mapper.MapToRequest(Is(record2)).Returns(request2);

            await handler.Handle(snsEvent, cancellationToken);

            await sqs.Received().SendMessageBatchAsync(Any<SendMessageBatchRequest>(), Is(cancellationToken));
            var request = TestUtils.GetArg<SendMessageBatchRequest>(sqs, nameof(IAmazonSQS.SendMessageBatchAsync), 0);
            request.Entries.Should().Contain(entry => entry.Id == request1.Id.ToString());
            request.Entries.Should().Contain(entry => entry.Id == request2.Id.ToString());
        }

        [Test, Auto]
        public async Task ShouldPublishMessagesWithQueueUrlToSQS(
            SNSEvent snsEvent,
            [Frozen] Options options,
            [Frozen] IAmazonSQS sqs,
            [Frozen] ISnsRecordMapper mapper,
            [Target] Handler handler,
            CancellationToken cancellationToken
        )
        {
            await handler.Handle(snsEvent, cancellationToken);

            await sqs.Received().SendMessageBatchAsync(Any<SendMessageBatchRequest>(), Is(cancellationToken));
            var request = TestUtils.GetArg<SendMessageBatchRequest>(sqs, nameof(IAmazonSQS.SendMessageBatchAsync), 0);
            request.QueueUrl.Should().Be(options.QueueUrl.ToString());
        }
    }
}
