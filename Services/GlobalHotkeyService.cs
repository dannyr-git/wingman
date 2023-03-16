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
        private readonly Dictionary<string, EventHandler> _hotkeyUpHandlers;
        private readonly Dictionary<string, EventHandler> _hotkeyDownHandlers;
        private readonly ILocalSettings _settingsService;
        private Func<string, bool> _keyConfigurationCallback;

        public GlobalHotkeyService(ILocalSettings settingsService)
        {
            _hook = Hook.GlobalEvents();
            _hotkeyUpHandlers = new Dictionary<string, EventHandler>();
            _hotkeyDownHandlers = new Dictionary<string, EventHandler>();
            _settingsService = settingsService;

            _hook.KeyDown += Hook_KeyDown;
            _hook.KeyUp += Hook_KeyUp;
        }

        public void RegisterHotkeyUp(string settingsKey, EventHandler handler)
        {
            if (!_hotkeyUpHandlers.ContainsKey(settingsKey))
            {
                _hotkeyUpHandlers[settingsKey] = null;
            }
            _hotkeyUpHandlers[settingsKey] += handler;
        }

        public void RegisterHotkeyDown(string settingsKey, EventHandler handler)
        {
            if (!_hotkeyDownHandlers.ContainsKey(settingsKey))
            {
                _hotkeyDownHandlers[settingsKey] = null;
            }
            _hotkeyDownHandlers[settingsKey] += handler;
        }

        public void UnregisterHotkeyUp(string settingsKey, EventHandler handler)
        {
            if (_hotkeyUpHandlers.ContainsKey(settingsKey))
            {
                _hotkeyUpHandlers[settingsKey] -= handler;
            }
        }

        public void UnregisterHotkeyDown(string settingsKey, EventHandler handler)
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

        private void HandleKeyEvent(KeyEventArgs e, Dictionary<string, EventHandler> handlers)
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


        private Dictionary<string, Keys> specialKeysMap = new Dictionary<string, Keys>
        {
            { "`", Keys.Oem3 }
/*
 * These need testing
 * 
    { ";", Keys.OemSemicolon },
    { ":", Keys.Oem1 },
    { "+", Keys.Oemplus },
    { ",", Keys.Oemcomma },
    { "-", Keys.OemMinus },
    { ".", Keys.OemPeriod },
    { "/", Keys.OemQuestion },
    { "?", Keys.Oem2 },
    { "`", Keys.Oem3 },
    { "~", Keys.Oemtilde },
    { "[", Keys.OemOpenBrackets },
    { "{", Keys.Oem4 },
    { "\\", Keys.OemPipe },
    { "|", Keys.Oem5 },
    { "]", Keys.OemCloseBrackets },
    { "}", Keys.Oem6 },
    { "'", Keys.OemQuotes },
    { "\"", Keys.Oem7 },
    { "Oem8", Keys.Oem8 }, // No specific character representation
    { "<", Keys.OemBackslash },
    { ">", Keys.Oem102 }
*/
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

            _hook.KeyDown -= Hook_KeyDown;
            _hook.KeyUp -= Hook_KeyUp;
            _hook.KeyDown += Hook_KeyDown_Configuration;

            while (_keyConfigurationCallback != null)
            {
                await Task.Delay(500);
            }

            _hook.KeyDown -= Hook_KeyDown_Configuration;
            _hook.KeyDown += Hook_KeyDown;
            _hook.KeyUp += Hook_KeyUp;
        }

        private void Hook_KeyDown_Configuration(object sender, KeyEventArgs e)
        {
            var key = e.KeyCode.ToString();

            if (key == "Escape" || string.IsNullOrEmpty(key))
            {
                _keyConfigurationCallback = null;
                return;
            }

            var result = _keyConfigurationCallback(key);

            if (result)
            {
                _keyConfigurationCallback = null;
            }
        }

    }
}
