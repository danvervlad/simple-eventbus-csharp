using System.Threading;
using System.Threading.Tasks;

namespace simple.eventbus.Decorators.Requester
{
    public interface IRequesterPublisher
    {
        Task<T> Request<T>(string topic, string eventName, object data = null, CancellationToken token = default);
    }
}