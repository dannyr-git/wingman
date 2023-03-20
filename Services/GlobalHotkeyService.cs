using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WindowsHook;
using wingman.Interfaces;
using static wingman.Helpers.KeyConverter;

namespace wingman.Services
{
    public class KeyCombination
    {
        public Keys KeyCode { get; }
        public ModifierKeys Modifiers { get; }
        public KeyCombination OriginalRecord { get; }

        public KeyCombination(Keys keyCode, ModifierKeys modifiers)
        {
            KeyCode = keyCode;
            Modifiers = modifiers;
            OriginalRecord = null;
        }

        public KeyCombination(Keys keyCode, ModifierKeys modifiers, KeyCombination originalRecord)
        {
            KeyCode = keyCode;
            Modifiers = modifiers;
            OriginalRecord = originalRecord;
        }

        public override bool Equals(object obj)
        {
            return obj is KeyCombination other && KeyCode == other.KeyCode && Modifiers == other.Modifiers;
        }

        public static bool operator ==(KeyCombination left, KeyCombination right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(KeyCombination left, KeyCombination right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(KeyCode, Modifiers);
        }
    }

    public enum HotkeyType
    {
        KeyDown,
        KeyUp
    }

    public class GlobalHotkeyService : IGlobalHotkeyService
    {
        private readonly IKeyboardMouseEvents _hook;
        private readonly Dictionary<WingmanSettings, EventHandler> _hotkeyUpHandlers;
        private readonly Dictionary<WingmanSettings, EventHandler> _hotkeyDownHandlers;
        private readonly ISettingsService _settingsService;
        private readonly Dictionary<HotkeyType, KeyCombination> _cachedValidHotkeys;
        private readonly HashSet<KeyCombination> _currentlyPressedCombinations;

        public GlobalHotkeyService(ISettingsService settingsService)
        {
            _hook = Hook.GlobalEvents();
            _hotkeyUpHandlers = new Dictionary<WingmanSettings, EventHandler>();
            _hotkeyDownHandlers = new Dictionary<WingmanSettings, EventHandler>();
            _settingsService = settingsService;

            _currentlyPressedKeys = new HashSet<Keys>();
            _cachedValidHotkeys = new Dictionary<HotkeyType, KeyCombination>();
            _currentlyPressedCombinations = new HashSet<KeyCombination>();

            _hook.KeyDown += Hook_KeyDown;
            _hook.KeyUp += Hook_KeyUp;
        }

        public void UpdateHotkeyCache() // could potentially speed up keyup/down events; not sure if it's worth it
        {
            foreach (var handlerEntry in _hotkeyUpHandlers)
            {
                WingmanSettings settingsKey = handlerEntry.Key;
                var hotkeyStr = _settingsService.Load<string>(settingsKey);
                var hotkeyCombination = ParseHotkeyCombination(hotkeyStr);

                _cachedValidHotkeys[HotkeyType.KeyUp] = hotkeyCombination;
            }
            foreach (var handlerEntry in _hotkeyDownHandlers)
            {
                WingmanSettings settingsKey = handlerEntry.Key;
                var hotkeyStr = _settingsService.Load<string>(settingsKey);
                var hotkeyCombination = ParseHotkeyCombination(hotkeyStr);

                _cachedValidHotkeys[HotkeyType.KeyDown] = hotkeyCombination;
            }

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
            if (!IsModifierKey(e.KeyCode))
            {
                var currentModifiers = GetCurrentModifiers(e);
                var keyCombination = new KeyCombination(e.KeyCode, currentModifiers, new KeyCombination(e.KeyCode, currentModifiers));
                _currentlyPressedCombinations.Add(keyCombination);
                if (HandleKeyEvent(keyCombination, _hotkeyDownHandlers))
                {
                    Console.WriteLine(String.Format("D: {0} has been handled with mods: {1}", e.KeyCode.ToString(), e.Modifiers.ToString()));
                    e.Handled = true;
                }
                else
                {
                    _currentlyPressedCombinations.Remove(keyCombination);
                }
            }
            else
            {
                // Ignore modifier keys by themselves
            }
        }

        private void Hook_KeyUp(object sender, KeyEventArgs e)
        {
            if (!IsModifierKey(e.KeyCode))
            {
                var findPressed = _currentlyPressedCombinations.FirstOrDefault(x => x.KeyCode == e.KeyCode);

                if (findPressed == null)
                    return;

                _currentlyPressedCombinations.Remove(findPressed);

                if (HandleKeyEvent(findPressed.OriginalRecord, _hotkeyUpHandlers))
                {
                    e.Handled = true;
                    Debug.WriteLine(String.Format("UpX. {0} is handled.", e.KeyCode.ToString()));
                }
            }
        }

        // takes KeyCombination
        private bool HandleKeyEvent(KeyCombination pressed, Dictionary<WingmanSettings, EventHandler> handlers)
        {
            bool isHandled = false;

            foreach (var handlerEntry in handlers)
            {
                var settingsKey = handlerEntry.Key;
                var handler = handlerEntry.Value;
                var hotkeySettingString = _settingsService.Load<string>(settingsKey);
                var hotkeyCombo = ParseHotkeyCombination(hotkeySettingString);


                if (hotkeyCombo == pressed)
                {
                    handler?.Invoke(this, EventArgs.Empty);
                    isHandled = true;
                }
            }
            return isHandled;
        }


        private readonly Dictionary<string, Keys> specialKeysMap = new Dictionary<string, Keys>
        {
            { "`", Keys.Oem3 },
            { ";", Keys.Oem1 },
            { "=", Keys.Oemplus },
            { ",", Keys.Oemcomma },
            { "-", Keys.OemMinus },
            { ".", Keys.OemPeriod },
            { "/", Keys.Oem2 },
            { "[", Keys.Oem4 },
            { "\\", Keys.Oem5 },
            { "]", Keys.Oem6 },
            { "'", Keys.Oem7 }
        };

        private KeyCombination ParseHotkeyCombination(string hotkeyCombination)
        {
            Keys newkey = Keys.None;
            ModifierKeys modifiers = ModifierKeys.None;

            if (hotkeyCombination.Length > 1 && hotkeyCombination.Contains('+'))
            {
                var keysAndModifiers = hotkeyCombination.Split("+");
                var keystr = keysAndModifiers.TakeLast(1).Single().Trim();
                Array.Resize(ref keysAndModifiers, keysAndModifiers.Length - 1);

                newkey = specialKeysMap.ContainsKey(keystr) ? specialKeysMap[keystr] : (Keys)Enum.Parse(typeof(Keys), keystr, ignoreCase: true);


                foreach (var modifier in keysAndModifiers)
                {
                    // Check if the key is a modifier key
                    if (modifier == "Alt")
                    {
                        modifiers |= ModifierKeys.Alt;
                    }
                    else if (modifier == "Ctrl")
                    {
                        modifiers |= ModifierKeys.Control;
                    }
                    else if (modifier == "Shift")
                    {
                        modifiers |= ModifierKeys.Shift;
                    }
                    else if (modifier == "Win")
                    {
                        modifiers |= ModifierKeys.Windows;
                    }
                }

            }
            else
            {
                modifiers = ModifierKeys.None;
                newkey = specialKeysMap.ContainsKey(hotkeyCombination) ? specialKeysMap[hotkeyCombination] : (Keys)Enum.Parse(typeof(Keys), hotkeyCombination, ignoreCase: true);
            }

            // Create the key combination
            return new KeyCombination(newkey, modifiers);
        }




        private ModifierKeys GetCurrentModifiers(KeyEventArgs e)
        {
            ModifierKeys currentModifiers = ModifierKeys.None;
            if (e.Control) currentModifiers |= ModifierKeys.Control;
            if (e.Shift) currentModifiers |= ModifierKeys.Shift;
            if (e.Alt) currentModifiers |= ModifierKeys.Alt;

            return currentModifiers;
        }

        // Start of Hotkey Configuration Routines

        private Func<string, bool> _keyConfigurationCallback;
        private readonly HashSet<Keys> _currentlyPressedKeys;

        public async Task ConfigureHotkeyAsync(Func<string, bool> keyConfigurationCallback)
        {
            _currentlyPressedKeys.Clear();
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
            if (!IsModifierKey(e.KeyCode))
            {
                _currentlyPressedKeys.Add(e.KeyCode);
            }

            ModifierKeys currentModifiers = ModifierKeys.None;
            if (e.Control) currentModifiers |= ModifierKeys.Control;
            if (e.Shift) currentModifiers |= ModifierKeys.Shift;
            if (e.Alt) currentModifiers |= ModifierKeys.Alt;

            var otherModifiers = GetCurrentModifiers(e);
            if ((otherModifiers & ModifierKeys.Windows) != 0)
                currentModifiers |= ModifierKeys.Windows;


            if (!IsModifierKey(e.KeyCode))
            {
                var hkstr = BuildHotkeyString(e);
                _keyConfigurationCallback?.Invoke(hkstr);
                _keyConfigurationCallback = null;
                e.Handled = true;
            }
        }


        private void Hook_KeyUp_Configuration(object sender, KeyEventArgs e)
        {
            _currentlyPressedKeys.Remove(e.KeyCode);
        }

        private bool IsModifierKey(Keys keyCode)
        {
            return keyCode == Keys.ControlKey || keyCode == Keys.ShiftKey || keyCode == Keys.Menu || keyCode == Keys.LMenu || keyCode == Keys.RMenu
                || keyCode == Keys.LShiftKey || keyCode == Keys.RShiftKey || keyCode == Keys.LControlKey || keyCode == Keys.RControlKey || keyCode == Keys.LWin || keyCode == Keys.RWin;
        }

        private string BuildHotkeyString(KeyEventArgs e)
        {
            List<string> keyParts = new List<string>();

            if (e.Control) keyParts.Add("Ctrl");
            if (e.Shift) keyParts.Add("Shift");
            if (e.Alt) keyParts.Add("Alt");
            var otherModifiers = GetCurrentModifiers(e);
            if ((otherModifiers & ModifierKeys.Windows) != 0)
                keyParts.Add("Win");

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
