using System;
using System.Collections.Generic;

namespace simple.eventbus
{
    public class EventBus : IEventBus
    {
        private readonly Bus _bus = new Bus();
        private readonly Queue<PostQueueItem> _postHandlers = new Queue<PostQueueItem>();

        public void Send(string topic, string eventName, object data = null)
        {
            var handlers = _bus.GetEventHandlers(topic, eventName);
            if (handlers == null)
            {
                throw new InvalidOperationException(
                    $"There are no handlers for topic: '{topic}', eventName: '{eventName}'");
            }

            foreach (var handler in handlers)
            {
                handler(data);
            }
        }

        public void Post(string topic, string eventName, object data = null)
        {
            var handlers = _bus.GetEventHandlers(topic, eventName);
            if (handlers == null)
            {
                throw new InvalidOperationException(
                    $"There are no handlers for topic: '{topic}', eventName: '{eventName}'");
            }

            _postHandlers.Enqueue(new PostQueueItem(handlers, data));
        }

        public IDisposable Subscribe(string topic, string eventName, Action<object> handler)
        {
            _bus.AddEventHandler(topic, eventName, handler);
            return new SubscribeToken(_bus, topic, eventName, handler);
        }

        public void ProceedQueue()
        {
            if (_postHandlers.Count == 0)
            {
                return;
            }

            var postQueueItem = _postHandlers.Dequeue();
            foreach (var handler in postQueueItem.Handlers)
            {
                handler(postQueueItem.Data);
            }
        }

        private readonly struct PostQueueItem
        {
            public readonly List<Action<object>> Handlers;
            public readonly object Data;

            public PostQueueItem(List<Action<object>> handlers, object data)
            {
                Handlers = handlers;
                Data = data;
            }
        }

        private class Bus : Dictionary<string, Dictionary<string, List<Action<object>>>>
        {
            public List<Action<object>> GetEventHandlers(string topic, string eventName)
            {
                if (!TryGetValue(topic, out var events))
                {
                    return null;
                }

                if (!events.TryGetValue(eventName, out var eventHandlers))
                {
                    return null;
                }

                return eventHandlers;
            }

            public void AddEventHandler(string topic, string eventName, Action<object> handler)
            {
                if (!TryGetValue(topic, out var events))
                {
                    events = new Dictionary<string, List<Action<object>>>();
                    events.Add(eventName, new List<Action<object>>() { handler } );
                    this.Add(topic, events);
                    return;
                }

                if (!events.TryGetValue(eventName, out var eventHandlers))
                {
                    events.Add(eventName, new List<Action<object>>() { handler } );
                    return;
                }

                eventHandlers.Add(handler);
            }

            public void RemoveEventHandler(string topic, string eventName, Action<object> handler)
            {
                if (!TryGetValue(topic, out var events))
                {
                    return;
                }

                if (!events.TryGetValue(eventName, out var eventHandlers))
                {
                    return;
                }

                eventHandlers.Remove(handler);
            }
        }

        private class SubscribeToken : IDisposable
        {
            private readonly Bus _bus;
            private readonly string _topic;
            private readonly string _eventName;
            private readonly Action<object> _handler;

            public SubscribeToken(Bus bus, string topic, string eventName, Action<object> handler)
            {
                _bus = bus;
                _topic = topic;
                _eventName = eventName;
                _handler = handler;
            }

            public void Dispose()
            {
                _bus.RemoveEventHandler(_topic, _eventName, _handler);
            }
        }
    }
}