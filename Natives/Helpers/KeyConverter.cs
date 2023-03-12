using System;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Input.Preview.Injection;


namespace wingman.Natives.Helpers
{
    public static class KeyConverter
    {
        [Flags]
        public enum ModifierKeys
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8,
        }

        public static List<InjectedInputKeyboardInfo> NormalizeKeystroke(string keystroke)
        {
            // Split the keystroke string into modifier keys and the main key
            string[] parts = keystroke.Split('+');
            ModifierKeys modifiers = ModifierKeys.None;
            string keyString = parts[parts.Length - 1];

            // Convert the modifier keys to a ModifierKeys enum value
            for (int i = 0; i < parts.Length - 1; i++)
            {
                switch (parts[i].ToLower())
                {
                    case "ctrl":
                    case "control":
                        modifiers |= ModifierKeys.Control;
                        break;
                    case "alt":
                        modifiers |= ModifierKeys.Alt;
                        break;
                    case "shift":
                        modifiers |= ModifierKeys.Shift;
                        break;
                    case "win":
                    case "windows":
                        modifiers |= ModifierKeys.Windows;
                        break;
                    default:
                        throw new ArgumentException("Invalid modifier key: " + parts[i]);
                }
            }

            // Convert the main key to a VirtualKey enum value
            VirtualKey key;
            switch (keyString.ToLower())
            {
                case "up":
                    key = VirtualKey.Up;
                    break;
                case "down":
                    key = VirtualKey.Down;
                    break;
                case "right":
                    key = VirtualKey.Right;
                    break;
                case "left":
                    key = VirtualKey.Left;
                    break;
                case "backspace":
                    key = VirtualKey.Back;
                    break;
                case "break":
                    key = VirtualKey.Cancel;
                    break;
                case "capslock":
                    key = VirtualKey.CapitalLock;
                    break;
                case "delete":
                    key = VirtualKey.Delete;
                    break;
                case "end":
                    key = VirtualKey.End;
                    break;
                case "enter":
                    key = VirtualKey.Enter;
                    break;
                case "esc":
                    key = VirtualKey.Escape;
                    break;
                case "help":
                    key = VirtualKey.Help;
                    break;
                case "home":
                    key = VirtualKey.Home;
                    break;
                case "insert":
                    key = VirtualKey.Insert;
                    break;
                case "numlock":
                    key = VirtualKey.NumberKeyLock;
                    break;
                case "pagedown":
                    key = VirtualKey.PageDown;
                    break;
                case "pageup":
                    key = VirtualKey.PageUp;
                    break;
                case "printscreen":
                    key = VirtualKey.Print;
                    break;
                case "scrolllock":
                    key = VirtualKey.Scroll;
                    break;
                case "space":
                    key = VirtualKey.Space;
                    break;
                case "tab":
                    key = VirtualKey.Tab;
                    break;
                case "f1":
                    key = VirtualKey.F1;
                    break;
                case "f2":
                    key = VirtualKey.F2;
                    break;
                case "f3":
                    key = VirtualKey.F3;
                    break;
                case "f4":
                    key = VirtualKey.F4;
                    break;
                case "f5":
                    key = VirtualKey.F5;
                    break;
                case "f6":
                    key = VirtualKey.F6;
                    break;
                case "f7":
                    key = VirtualKey.F7;
                    break;
                case "f8":
                    key = VirtualKey.F8;
                    break;
                case "f9":
                    key = VirtualKey.F9;
                    break;
                case "f10":
                    key = VirtualKey.F10;
                    break;
                case "f11":
                    key = VirtualKey.F11;
                    break;
                case "f12":
                    key = VirtualKey.F12;
                    break;
                case "f13":
                    key = VirtualKey.F13;
                    break;
                case "f14":
                    key = VirtualKey.F14;
                    break;
                case "f15":
                    key = VirtualKey.F15;
                    break;
                case "f16":
                    key = VirtualKey.F16;
                    break;
                default:
                    if (keyString.Length == 1)
                    {
                        key = (VirtualKey)keyString.ToUpper()[0];
                    }
                    else
                    {
                        throw new ArgumentException("Invalid key: " + keyString);
                    }
                    break;
            }

            // Convert the modifier keys and main key to an input buffer
            List<InjectedInputKeyboardInfo> inputBuffer = new List<InjectedInputKeyboardInfo>();
            if (modifiers.HasFlag(ModifierKeys.Control))
            {
                inputBuffer.Add(new InjectedInputKeyboardInfo { VirtualKey = (ushort)VirtualKey.Control, KeyOptions = InjectedInputKeyOptions.None });
            }
            if (modifiers.HasFlag(ModifierKeys.Alt))
            {
                inputBuffer.Add(new InjectedInputKeyboardInfo { VirtualKey = (ushort)VirtualKey.Menu, KeyOptions = InjectedInputKeyOptions.None });
            }
            if (modifiers.HasFlag(ModifierKeys.Shift))
            {
                inputBuffer.Add(new InjectedInputKeyboardInfo { VirtualKey = (ushort)VirtualKey.Shift, KeyOptions = InjectedInputKeyOptions.None });
            }
            if (modifiers.HasFlag(ModifierKeys.Windows))
            {
                inputBuffer.Add(new InjectedInputKeyboardInfo { VirtualKey = (ushort)VirtualKey.LeftWindows, KeyOptions = InjectedInputKeyOptions.None });
            }

            inputBuffer.Add(new InjectedInputKeyboardInfo { VirtualKey = (ushort)key, KeyOptions = InjectedInputKeyOptions.None });

            if (modifiers.HasFlag(ModifierKeys.Windows))
            {
                inputBuffer.Add(new InjectedInputKeyboardInfo { VirtualKey = (ushort)VirtualKey.LeftWindows, KeyOptions = InjectedInputKeyOptions.KeyUp });
            }
            if (modifiers.HasFlag(ModifierKeys.Shift))
            {
                inputBuffer.Add(new InjectedInputKeyboardInfo { VirtualKey = (ushort)VirtualKey.Shift, KeyOptions = InjectedInputKeyOptions.KeyUp });
            }
            if (modifiers.HasFlag(ModifierKeys.Alt))
            {
                inputBuffer.Add(new InjectedInputKeyboardInfo { VirtualKey = (ushort)VirtualKey.Menu, KeyOptions = InjectedInputKeyOptions.KeyUp });
            }
            if (modifiers.HasFlag(ModifierKeys.Control))
            {
                inputBuffer.Add(new InjectedInputKeyboardInfo { VirtualKey = (ushort)VirtualKey.Control, KeyOptions = InjectedInputKeyOptions.KeyUp });
            }

            return inputBuffer;
        }
    }
}
