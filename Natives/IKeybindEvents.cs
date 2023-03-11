using System;
using System.Threading.Tasks;

namespace wingman.Natives
{
    public interface IKeybindEvents
    {
        bool Enabled { get; set; }

        event Func<Task<bool>> OnMainHotkey;
        event Func<Task<bool>> OnMainHotkeyRelease;

        event Func<Task<bool>> OnModalHotkey;
        event Func<Task<bool>> OnModalHotkeyRelease;
        public void ToggleKeybinds();
    }
}
