using Action.Events;
using Action.Handlers;
using CountingLibrary.Core;
using CountingLibrary.Handlers;

namespace CountingLibrary.Events
{
    public class ChangeProcessingTypeEvent : Event
    {
        public ProcessingType ProcessingType { get; set; }
        
        public ChangeProcessingTypeEvent(ProcessingType processingType)
        {
            ProcessingType = processingType;
        }

        public override void Execute(IEventHandler eventHandler)
        {
            ((IEventHandlerChangeProcessingType)eventHandler).OnChangeProcessingType(this);
        }
    }
}