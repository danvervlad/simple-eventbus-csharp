using simple.eventbus.Decorators.Requester;

namespace simple.eventbus.Facade
{
    public interface IEventBusFacade : IEventBus, IRequester
    {
    }
}