using Xunit;

namespace simple.eventbus.tests
{
    public class Tests
    {
        [Fact]
        public void Send()
        {
            // arrange
            var eventBus = new EventBus();
            var payloadData = string.Empty;
            eventBus.Subscribe("topic", "eventName", data => payloadData = (string)data);

            // act
            eventBus.Send("topic", "eventName", "payloadData");

            // assert
            Assert.True(payloadData == "payloadData");
        }

        [Fact]
        public void Post()
        {
            // arrange
            var eventBus = new EventBus();
            var payloadData = string.Empty;
            eventBus.Subscribe("topic", "eventName", data => payloadData = (string)data);

            // act
            eventBus.Post("topic", "eventName", "payloadData");

            // assert
            Assert.True(string.IsNullOrEmpty(payloadData));
            eventBus.ProceedQueue();
            Assert.True(payloadData == "payloadData");
        }
    }
}