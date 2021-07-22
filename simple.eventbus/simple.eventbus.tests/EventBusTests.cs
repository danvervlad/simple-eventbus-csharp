using Xunit;

namespace simple.eventbus.tests
{
    public class EventBusTests
    {
        [Fact]
        public void SendSuccessful()
        {
            // arrange
            var eventBus = new EventBus();
            var payloadData = string.Empty;
            eventBus.Subscribe("topic", "eventName", data => payloadData = (string)data);

            // act
            var hasAnyHandlers = eventBus.Send("topic", "eventName", "payloadData");

            // assert
            Assert.True(hasAnyHandlers);
            Assert.True(payloadData == "payloadData");
        }

        [Fact]
        public void SendSuccessfulWithMultipleSubscriptions()
        {
            // arrange
            var eventBus = new EventBus();
            var handlerCount = 0;
            eventBus.Subscribe("topic", "eventName", data => handlerCount++);
            eventBus.Subscribe("topic", "eventName", data => handlerCount++);
            eventBus.Subscribe("topic", "eventName", data => handlerCount++);

            // act
            var hasAnyHandlers = eventBus.Send("topic", "eventName", "payloadData");

            // assert
            Assert.True(hasAnyHandlers);
            Assert.True(handlerCount == 3);
        }

        [Fact]
        public void SendFailedCauseNoHandlers()
        {
            // arrange
            var eventBus = new EventBus();

            // act
            var hasAnyHandlers = eventBus.Send("topic", "eventName", "payloadData");

            // assert
            Assert.False(hasAnyHandlers);
        }

        [Fact]
        public void SendFailedCauseNoHandlersAfterTokenDispose()
        {
            // arrange
            var eventBus = new EventBus();
            var payloadData = string.Empty;
            var token = eventBus.Subscribe("topic", "eventName", data => payloadData = (string)data);

            // act
            token.Dispose();
            var hasAnyHandlers = eventBus.Send("topic", "eventName", "payloadData");

            // assert
            Assert.False(hasAnyHandlers);
            Assert.True(string.IsNullOrEmpty(payloadData));
        }

        [Fact]
        public void PostSuccessful()
        {
            // arrange
            var eventBus = new EventBus();
            var payloadData = string.Empty;
            eventBus.Subscribe("topic", "eventName", data => payloadData = (string)data);

            // act
            eventBus.Post("topic", "eventName", "payloadData");
            eventBus.ProceedQueue();

            // assert
            Assert.True(payloadData == "payloadData");
        }

        [Fact]
        public void PostFailedCauseNoProceedQueue()
        {
            // arrange
            var eventBus = new EventBus();
            var payloadData = string.Empty;
            eventBus.Subscribe("topic", "eventName", data => payloadData = (string)data);

            // act
            eventBus.Post("topic", "eventName", "payloadData");

            // assert
            Assert.True(string.IsNullOrEmpty(payloadData));
        }

        [Fact]
        public void PostFailedCauseNoHandlers()
        {
            // arrange
            var eventBus = new EventBus();

            // act
            var hasAnyHandlers = eventBus.Post("topic", "eventName", "payloadData");
            eventBus.ProceedQueue();

            // assert
            Assert.False(hasAnyHandlers);
        }

        [Fact]
        public void PostFailedCauseNoHandlersAfterTokenDispose()
        {
            // arrange
            var eventBus = new EventBus();
            var payloadData = string.Empty;
            var token = eventBus.Subscribe("topic", "eventName", data => payloadData = (string)data);

            // act
            token.Dispose();
            var hasAnyHandlers = eventBus.Post("topic", "eventName", "payloadData");
            eventBus.ProceedQueue();

            // assert
            Assert.False(hasAnyHandlers);
            Assert.True(string.IsNullOrEmpty(payloadData));
        }
    }
}