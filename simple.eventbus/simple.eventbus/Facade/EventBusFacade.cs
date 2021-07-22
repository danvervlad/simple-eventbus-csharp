using System;
using System.Threading;
using System.Threading.Tasks;
using simple.eventbus.Decorators.Requester;

namespace simple.eventbus.Facade
{
    public class EventBusFacade : IEventBusFacade
    {
        private readonly IEventBus _eventBus;
        private readonly IRequester _requester;

        public EventBusFacade()
        {
            _eventBus = new EventBus();
            _requester = new Requester(_eventBus);
        }

        public IDisposable Subscribe(string topic, string eventName, Action<object> handler)
        {
            return _eventBus.Subscribe(topic, eventName, handler);
        }

        public bool Send(string topic, string eventName, object data = null)
        {
            return _eventBus.Send(topic, eventName, data);
        }

        public bool Post(string topic, string eventName, object data = null)
        {
            return _eventBus.Post(topic, eventName, eventName);
        }

        public void ProceedQueue()
        {
            _eventBus.ProceedQueue();
        }

        public Task<T> Request<T>(string topic, string eventName, object data = null, CancellationToken token = default)
        {
            return _requester.Request<T>(topic, eventName, data, token);
        }

        public IDisposable SubscribeForReply<T>(string topic, string eventName, Func<object, CancellationToken, Task<T>> handler)
        {
            return _requester.SubscribeForReply<T>(topic, eventName, handler);
        }
    }
}