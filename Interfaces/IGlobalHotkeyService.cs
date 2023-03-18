using System;
using System.Threading.Tasks;
using wingman.Services;

namespace wingman.Interfaces
{
    public interface IGlobalHotkeyService
    {
        Task ConfigureHotkeyAsync(Func<string, bool> keyConfigurationCallback);

        void RegisterHotkeyUp(WingmanSettings settingsKey, EventHandler handler);
        void RegisterHotkeyDown(WingmanSettings settingsKey, EventHandler handler);
        void UnregisterHotkeyUp(WingmanSettings settingsKey, EventHandler handler);
        void UnregisterHotkeyDown(WingmanSettings settingsKey, EventHandler handler);
    }
}
