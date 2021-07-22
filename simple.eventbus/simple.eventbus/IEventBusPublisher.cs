namespace simple.eventbus
{
    public interface IEventBusPublisher
    {
        void Send(string topic, string eventName, object data = null);
        void Post(string topic, string eventName, object data = null);
    }
}