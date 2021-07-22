namespace simple.eventbus
{
    public interface IEventBus : IEventBusSubscriber, IEventBusPublisher
    {
        void ProceedQueue();
    }
}