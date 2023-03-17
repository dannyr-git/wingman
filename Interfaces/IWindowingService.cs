using System;
using System.Threading.Tasks;

namespace wingman.Interfaces
{
    public interface IWindowingService
    {
        Task CreateModal(string content, string title = "Modal", int width = 640, int height = 480, bool isresizable = true, bool activated = true);
        Task CreateCodeModal(string content);
        void UpdateStatus(string currentStatus);
        void ForceStatusHide();
        event EventHandler<string> EventStatusChanged;
        event EventHandler EventForceStatusHide;
    }
}
