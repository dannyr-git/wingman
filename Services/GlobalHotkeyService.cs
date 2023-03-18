using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WindowsHook;
using wingman.Interfaces;
using static wingman.Helpers.KeyConverter;

namespace wingman.Services
{
    public class GlobalHotkeyService : IGlobalHotkeyService
    {
        private readonly IKeyboardMouseEvents _hook;
        private readonly Dictionary<WingmanSettings, EventHandler> _hotkeyUpHandlers;
        private readonly Dictionary<WingmanSettings, EventHandler> _hotkeyDownHandlers;
        private readonly ISettingsService _settingsService;
        private Func<string, bool> _keyConfigurationCallback;

        public GlobalHotkeyService(ISettingsService settingsService)
        {
            _hook = Hook.GlobalEvents();
            _hotkeyUpHandlers = new Dictionary<WingmanSettings, EventHandler>();
            _hotkeyDownHandlers = new Dictionary<WingmanSettings, EventHandler>();
            _settingsService = settingsService;

            _hook.KeyDown += Hook_KeyDown;
            _hook.KeyUp += Hook_KeyUp;
        }

        public void RegisterHotkeyUp(WingmanSettings settingsKey, EventHandler handler)
        {
            if (!_hotkeyUpHandlers.ContainsKey(settingsKey))
            {
                _hotkeyUpHandlers[settingsKey] = null;
            }
            _hotkeyUpHandlers[settingsKey] += handler;
        }

        public void RegisterHotkeyDown(WingmanSettings settingsKey, EventHandler handler)
        {
            if (!_hotkeyDownHandlers.ContainsKey(settingsKey))
            {
                _hotkeyDownHandlers[settingsKey] = null;
            }
            _hotkeyDownHandlers[settingsKey] += handler;
        }

        public void UnregisterHotkeyUp(WingmanSettings settingsKey, EventHandler handler)
        {
            if (_hotkeyUpHandlers.ContainsKey(settingsKey))
            {
                _hotkeyUpHandlers[settingsKey] -= handler;
            }
        }

        public void UnregisterHotkeyDown(WingmanSettings settingsKey, EventHandler handler)
        {
            if (_hotkeyDownHandlers.ContainsKey(settingsKey))
            {
                _hotkeyDownHandlers[settingsKey] -= handler;
            }
        }

        private void Hook_KeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyEvent(e, _hotkeyDownHandlers);
        }

        private void Hook_KeyUp(object sender, KeyEventArgs e)
        {
            HandleKeyEvent(e, _hotkeyUpHandlers);
        }

        private void HandleKeyEvent(KeyEventArgs e, Dictionary<WingmanSettings, EventHandler> handlers)
        {
            bool isHandled = false;
            foreach (var handlerEntry in handlers)
            {
                var settingsKey = handlerEntry.Key;
                var handler = handlerEntry.Value;
                var hotkeyCombination = _settingsService.Load<string>(settingsKey);

                if (IsHotkeyCombinationPressed(hotkeyCombination, e))
                {
                    handler?.Invoke(this, EventArgs.Empty);
                    isHandled = true;
                }
            }

            if (isHandled)
            {
                e.Handled = true;
            }
        }


        private readonly Dictionary<string, Keys> specialKeysMap = new Dictionary<string, Keys>
        {
            { "`", Keys.Oem3 }
        };

        private bool IsHotkeyCombinationPressed(string hotkeyCombination, KeyEventArgs e)
        {
            var keyParts = hotkeyCombination.Split('+');
            var keyModifiers = keyParts.Take(keyParts.Length - 1).Select(x => x.Trim());
            var mainKey = keyParts.Last().Trim();

            Keys mainKeyEnum;
            if (!specialKeysMap.TryGetValue(mainKey, out mainKeyEnum))
            {
                if (!Enum.TryParse(mainKey, out mainKeyEnum))
                {
                    throw new ArgumentException($"Invalid key: {mainKey}");
                }
            }

            ModifierKeys modifiersEnum = ModifierKeys.None;
            foreach (var modifier in keyModifiers)
            {
                ModifierKeys modifierEnum;
                if (!Enum.TryParse(modifier, out modifierEnum))
                {
                    throw new ArgumentException($"Invalid modifier: {modifier}");
                }

                modifiersEnum |= modifierEnum;
            }

            ModifierKeys currentModifiers = ModifierKeys.None;
            if (e.Control) currentModifiers |= ModifierKeys.Control;
            if (e.Shift) currentModifiers |= ModifierKeys.Shift;
            if (e.Alt) currentModifiers |= ModifierKeys.Alt;

            if (modifiersEnum == ModifierKeys.None)
            {
                return e.KeyCode == mainKeyEnum && currentModifiers == ModifierKeys.None;
            }
            else
            {
                return e.KeyCode == mainKeyEnum && (currentModifiers & modifiersEnum) == modifiersEnum;
            }
        }

        public enum HotkeyType
        {
            KeyDown,
            KeyUp
        }


        public async Task ConfigureHotkeyAsync(Func<string, bool> keyConfigurationCallback)
        {
            _keyConfigurationCallback = keyConfigurationCallback;

            // Unregister the original KeyDown and KeyUp listeners.
            _hook.KeyDown -= Hook_KeyDown;
            _hook.KeyUp -= Hook_KeyUp;

            // Register the configuration KeyDown and KeyUp listeners.
            _hook.KeyDown += Hook_KeyDown_Configuration;
            _hook.KeyUp += Hook_KeyUp_Configuration;

            while (_keyConfigurationCallback != null)
            {
                await Task.Delay(500);
            }

            // Unregister the configuration KeyDown and KeyUp listeners.
            _hook.KeyDown -= Hook_KeyDown_Configuration;
            _hook.KeyUp -= Hook_KeyUp_Configuration;

            // Re-register the original KeyDown and KeyUp listeners.
            _hook.KeyDown += Hook_KeyDown;
            _hook.KeyUp += Hook_KeyUp;
        }

        private void Hook_KeyDown_Configuration(object sender, KeyEventArgs e)
        {
            ModifierKeys currentModifiers = ModifierKeys.None;
            if (e.Control) currentModifiers |= ModifierKeys.Control;
            if (e.Shift) currentModifiers |= ModifierKeys.Shift;
            if (e.Alt) currentModifiers |= ModifierKeys.Alt;

            if ((currentModifiers != ModifierKeys.None && !IsModifierKey(e.KeyCode)) || (currentModifiers == ModifierKeys.None && !IsModifierKey(e.KeyCode)))
            {
                var hkstr = BuildHotkeyString(e);
                _keyConfigurationCallback?.Invoke(hkstr);
                _keyConfigurationCallback = null;
                e.Handled = true;
            }
        }

        private void Hook_KeyUp_Configuration(object sender, KeyEventArgs e)
        {

        }

        private bool IsModifierKey(Keys keyCode)
        {
            return keyCode == Keys.ControlKey || keyCode == Keys.ShiftKey || keyCode == Keys.Menu || keyCode == Keys.LMenu || keyCode == Keys.RMenu
                || keyCode == Keys.LShiftKey || keyCode == Keys.RShiftKey || keyCode == Keys.LControlKey || keyCode == Keys.RControlKey;
        }

        private string BuildHotkeyString(KeyEventArgs e)
        {
            List<string> keyParts = new List<string>();

            if (e.Control) keyParts.Add("Ctrl");
            if (e.Shift) keyParts.Add("Shift");
            if (e.Alt) keyParts.Add("Alt");

            string mainKey = specialKeysMap.FirstOrDefault(x => x.Value == e.KeyCode).Key;
            if (string.IsNullOrEmpty(mainKey))
            {
                mainKey = e.KeyCode.ToString();
            }

            keyParts.Add(mainKey);

            return string.Join("+", keyParts);
        }







    }
}
