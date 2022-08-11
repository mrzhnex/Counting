using Action.Handlers;
using CountingLibrary.Events;

namespace CountingLibrary.Handlers
{
    public interface IEventHandlerSort : IEventHandler
    {
        void OnSort(SortEvent sortEvent);
    }
}