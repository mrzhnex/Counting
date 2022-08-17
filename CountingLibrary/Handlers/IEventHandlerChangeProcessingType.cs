using Action.Handlers;
using CountingLibrary.Events;

namespace CountingLibrary.Handlers
{
    public interface IEventHandlerChangeProcessingType : IEventHandler
    {
        void OnChangeProcessingType(ChangeProcessingTypeEvent changeProcessingTypeEvent);
    }
}