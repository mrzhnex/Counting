using Action.Events;
using Action.Handlers;
using CountingLibrary.Handlers;
using CountingLibrary.Main;

namespace CountingLibrary.Events
{
    public class SortEvent : Event
    {
        public Sort Sort { get; set; }
        public SortEvent(Sort sort)
        {
            Sort = sort;
        }

        public override void Execute(IEventHandler eventHandler)
        {
            ((IEventHandlerSort)eventHandler).OnSort(this);
        }
    }
}