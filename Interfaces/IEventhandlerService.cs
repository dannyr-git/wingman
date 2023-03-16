using System;

namespace wingman.Interfaces
{
    public interface IEventHandlerService
    {
        EventHandler<bool> InferenceCallback { get; set; }
    }
}
