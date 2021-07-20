using System;

namespace simple.eventbus
{
    public interface IEventBus
    {
        void Send(string topic, string eventName, object data = null);
        void Post(string topic, string eventName, object data = null);
        IDisposable Subscribe(string topic, string eventName, Action<object> handler);
    }
}