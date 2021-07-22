using System;
using System.Collections.Generic;

namespace simple.eventbus
{
    public class EventBus : IEventBus
    {
        private readonly Bus _bus = new Bus();
        private readonly Queue<PostQueueItem> _postHandlers = new Queue<PostQueueItem>();

        public bool Send(string topic, string eventName, object data = null)
        {
            var handlers = _bus.GetEventHandlers(topic, eventName);
            if (handlers == null || handlers.Count == 0)
            {
                return false;
            }

            foreach (var handler in handlers)
            {
                handler(data);
            }

            return true;
        }

        public bool Post(string topic, string eventName, object data = null)
        {
            var handlers = _bus.GetEventHandlers(topic, eventName);
            if (handlers == null || handlers.Count == 0)
            {
                return false;
            }

            _postHandlers.Enqueue(new PostQueueItem(topic, eventName, data));
            return true;
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
            var handlers = _bus.GetEventHandlers(postQueueItem.Topic, postQueueItem.EventName);
            if (handlers == null || handlers.Count == 0)
            {
                return;
            }

            foreach (var handler in handlers)
            {
                handler(postQueueItem.Data);
            }
        }

        private readonly struct PostQueueItem
        {
            public readonly string Topic;
            public readonly string EventName;
            public readonly object Data;

            public PostQueueItem(string topic, string eventName, object data)
            {
                Topic = topic;
                EventName = eventName;
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
                    Add(topic, events);
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
                if (eventHandlers.Count == 0)
                {
                    events.Remove(eventName);
                }

                if (events.Count == 0)
                {
                    Remove(topic);
                }
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