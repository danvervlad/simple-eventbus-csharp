using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace simple.eventbus.Decorators.Requester
{
    public class Requester : IRequester
    {
        private readonly IEventBus _eventBus;

        public Requester(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public Task<T> Request<T>(string topic, string eventName, object data = null, CancellationToken token = default)
        {
            var replyGuid = Guid.NewGuid().ToString("N");
            var replyTopic = $"{topic}_{replyGuid}";
            var replySuccessEventName = $"{eventName}_{replyGuid}_success";
            var replyErrorEventName = $"{eventName}_{replyGuid}_error";

            var taskCompletionSource = new TaskCompletionSource<T>();
            var replySubscribeTokens = new List<IDisposable>(2);
            SubscribeOnReplySuccessEvent(replyTopic, replySuccessEventName, taskCompletionSource, replySubscribeTokens);
            SubscribeOnReplyErrorEvent(replyTopic, replyErrorEventName, taskCompletionSource, replySubscribeTokens);

            var requestPayload = new ReplyPayload(
                replyTopic,
                replySuccessEventName,
                replyErrorEventName,
                data,
                token == default ? CancellationToken.None : token);

            var handlersExist = _eventBus.Send(topic, eventName, requestPayload);
            if (handlersExist)
            {
                return taskCompletionSource.Task;
            }

            foreach (var replySubscribeToken in replySubscribeTokens)
            {
                replySubscribeToken.Dispose();
            }

            return Task.FromResult(default(T));
        }

        public IDisposable SubscribeForReply<T>(string topic, string eventName, Func<object, CancellationToken, Task<T>> handler)
        {
            return _eventBus.Subscribe(topic, eventName, async payload =>
            {
                if (!(payload is ReplyPayload replyPayload))
                {
                    return;
                }

                T replyResult;
                try
                {
                    replyResult = await handler(replyPayload.Payload, replyPayload.Token);
                }
                catch (Exception exception)
                {
                    _eventBus.Send(replyPayload.Topic, replyPayload.ErrorEventName, exception.ToString());
                    return;
                }

                _eventBus.Send(replyPayload.Topic, replyPayload.SuccessEventName, replyResult);
            });
        }

        private void SubscribeOnReplyErrorEvent<T>(
            string replyTopic,
            string replyErrorEventName,
            TaskCompletionSource<T> taskCompletionSource,
            List<IDisposable> replySubscribeTokens)
        {
            var requestErrorSubscribeToken = _eventBus.Subscribe(replyTopic, replyErrorEventName, payloadData =>
            {
                taskCompletionSource.SetException(new ReplierException((string) payloadData));
                foreach (var replySubscribeToken in replySubscribeTokens)
                {
                    replySubscribeToken.Dispose();
                }
            });
            replySubscribeTokens.Add(requestErrorSubscribeToken);
        }

        private void SubscribeOnReplySuccessEvent<T>(
            string replyTopic,
            string replySuccessEventName,
            TaskCompletionSource<T> taskCompletionSource,
            List<IDisposable> replySubscribeTokens)
        {
            var replySuccessSubscribeToken = _eventBus.Subscribe(replyTopic, replySuccessEventName, payloadData =>
            {
                try
                {
                    var replyData = (T) payloadData;
                    taskCompletionSource.SetResult(replyData);
                }
                catch (Exception exception)
                {
                    taskCompletionSource.SetException(new IncorrectReplyTypeException(exception.Message, exception));
                }

                foreach (var replySubscribeToken in replySubscribeTokens)
                {
                    replySubscribeToken.Dispose();
                }
            });
            replySubscribeTokens.Add(replySuccessSubscribeToken);
        }

        private class ReplyPayload
        {
            public readonly string Topic;
            public readonly string SuccessEventName;
            public readonly string ErrorEventName;
            public readonly object Payload;
            public readonly CancellationToken Token;

            public ReplyPayload(string topic, string successEventName, string errorEventName, object payload, CancellationToken token)
            {
                Topic = topic;
                SuccessEventName = successEventName;
                Payload = payload;
                ErrorEventName = errorEventName;
                Token = token;
            }
        }
    }
}