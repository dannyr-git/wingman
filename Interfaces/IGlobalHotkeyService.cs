using System;
using System.Threading.Tasks;

namespace wingman.Interfaces
{
    public interface IGlobalHotkeyService
    {
        Task ConfigureHotkeyAsync(Func<string, bool> keyConfigurationCallback);

        void RegisterHotkeyUp(string settingsKey, EventHandler handler);
        void RegisterHotkeyDown(string settingsKey, EventHandler handler);
        void UnregisterHotkeyUp(string settingsKey, EventHandler handler);
        void UnregisterHotkeyDown(string settingsKey, EventHandler handler);
    }
}
