using System;
using System.Threading.Tasks;
using simple.eventbus.Decorators.Requester;
using Xunit;

namespace simple.eventbus.tests
{
    public class RequesterTests
    {
        [Fact]
        public async Task RequestSuccessful()
        {
            // arrange
            IRequester requester = new Requester(new EventBus());
            var expectedResult = "payloadData_reply";
            requester.SubscribeForReply("topic", "eventName", (data, token) => Task.FromResult($"{data}_reply"));

            // act
            var result = await requester.Request<string>("topic", "eventName", "payloadData");

            // assert
            Assert.True(expectedResult == result);
        }

        [Fact]
        public async Task RequestSuccessWithNoRepliers()
        {
            // arrange
            IRequester requester = new Requester(new EventBus());

            // act
            var result = await requester.Request<string>("topic", "eventName", "payloadData");

            // assert
            Assert.True(result == default(string));
        }

        [Fact]
        public async Task RequestFailedWithIncorrectReplyType()
        {
            // arrange
            IRequester requester = new Requester(new EventBus());
            requester.SubscribeForReply("topic", "eventName", (data, token) => Task.FromResult(10));

            // act
            var exception =
                await Record.ExceptionAsync(() => requester.Request<string>("topic", "eventName", "payloadData"));

            // assert
            Assert.True(exception != null && exception is IncorrectReplyTypeException);
        }

        [Fact]
        public async Task RequestSuccessfulWithFirstReplyFromMultiple()
        {
            // arrange
            IRequester requester = new Requester(new EventBus());
            var token1 = requester.SubscribeForReply("topic", "eventName", (data, token) => Task.FromResult((int)data * 20));
            var token2 =requester.SubscribeForReply("topic", "eventName", (data, token) => Task.FromResult((int)data * 30));
            var token3 =requester.SubscribeForReply("topic", "eventName", (data, token) => Task.FromResult((int)data * 40));

            // act
            var result = await requester.Request<int>("topic", "eventName", 10);
            token1.Dispose();
            token2.Dispose();
            token3.Dispose();

            // assert
            Assert.True(result == 200);
        }

        [Fact]
        public async Task RequestFailedWithExceptionOnReplierSide()
        {
            // arrange
            IRequester requester = new Requester(new EventBus());
            requester.SubscribeForReply<string>("topic", "eventName", (data, token) => throw new InvalidOperationException("Something wrong"));

            // act
            var exception =
                await Record.ExceptionAsync(() => requester.Request<string>("topic", "eventName", "payloadData"));

            // assert
            Assert.True(exception != null && exception is ReplierException);
        }
    }
}