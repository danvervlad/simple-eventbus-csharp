using System;

namespace simple.eventbus
{
    public interface IEventBusSubscriber
    {
        IDisposable Subscribe(string topic, string eventName, Action<object> handler);
    }
}