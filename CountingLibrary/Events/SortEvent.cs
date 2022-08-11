using Action.Events;
using Action.Handlers;
using CountingLibrary.Handlers;

namespace CountingLibrary.Events
{
    public class SortEvent : Event
    {
        public override void Execute(IEventHandler eventHandler)
        {
            ((IEventHandlerSort)eventHandler).OnSort(this);
        }
    }
}