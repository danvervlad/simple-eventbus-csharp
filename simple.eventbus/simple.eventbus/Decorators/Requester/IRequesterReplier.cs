using System;
using System.Threading;
using System.Threading.Tasks;

namespace simple.eventbus.Decorators.Requester
{
    public interface IRequesterReplier
    {
        IDisposable SubscribeForReply<T>(string topic, string eventName, Func<object, CancellationToken, Task<T>> handler);
    }
}