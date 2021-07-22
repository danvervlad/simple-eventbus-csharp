namespace simple.eventbus
{
    public interface IEventBusPublisher
    {
        bool Send(string topic, string eventName, object data = null);
        bool Post(string topic, string eventName, object data = null);
    }
}