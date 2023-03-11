using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using wingman.Natives.Helpers;

namespace wingman.Natives
{
    public class NativeKeyboard : INativeKeyboard, IDisposable
    {
        private static readonly List<WindowsHook.Keys> KEYS_INVALID = new List<WindowsHook.Keys>() {
            WindowsHook.Keys.ControlKey,
            WindowsHook.Keys.LControlKey,
            WindowsHook.Keys.RControlKey,
            WindowsHook.Keys.ShiftKey,
            WindowsHook.Keys.RShiftKey,
            WindowsHook.Keys.LShiftKey,
            WindowsHook.Keys.RWin,
            WindowsHook.Keys.LWin,
            WindowsHook.Keys.LMenu,
            WindowsHook.Keys.RMenu,
        };

        private readonly ILogger logger;
        private readonly HookProvider hookProvider;

        public NativeKeyboard(HookProvider hookProvider)
        {
            this.hookProvider = hookProvider;
            hookProvider.Hook.KeyDown += Hook_KeyDown;
            hookProvider.Hook.KeyUp += Hook_KeyUp;
        }

        public bool Enabled { get; set; }

        public event Func<string, bool> OnKeyDown;
        public event Func<string, bool> OnKeyUp;

        private bool isDisposed;

        private async void Hook_KeyUp(object sender, WindowsHook.KeyEventArgs e)
        {
            if (KEYS_INVALID.Contains(e.KeyCode))
            {
                return;
            }

            // Transfer the event key to a string to compare to settings
            var str = new StringBuilder();
            if (e.Modifiers.HasFlag(WindowsHook.Keys.Control))
            {
                str.Append("Ctrl+");
            }
            if (e.Modifiers.HasFlag(WindowsHook.Keys.Shift))
            {
                str.Append("Shift+");
            }
            if (e.Modifiers.HasFlag(WindowsHook.Keys.Alt))
            {
                str.Append("Alt+");
            }
            if (e.Modifiers.HasFlag(WindowsHook.Keys.LWin) || e.Modifiers.HasFlag(WindowsHook.Keys.RWin))
            {
                str.Append("Win+");
            }

            if (e.KeyCode == WindowsHook.Keys.Oem3)
                str.Append("`");
            else
                str.Append(e.KeyCode);

            if (OnKeyDown != null)
            {
                str = str
                    .Replace("Back", "Backspace")
                    .Replace("Capital", "CapsLock")
                    .Replace("Next", "PageDown")
                    .Replace("Pause", "Break");

                bool result = true;

                if (OnKeyUp != null)
                    result = OnKeyUp.Invoke(str.ToString());

                if (result)
                {
                    e.Handled = true;
                }

            }
        }

        private async void Hook_KeyDown(object sender, WindowsHook.KeyEventArgs e)
        {
            if (KEYS_INVALID.Contains(e.KeyCode))
            {
                return;
            }

            // Transfer the event key to a string to compare to settings
            var str = new StringBuilder();
            if (e.Modifiers.HasFlag(WindowsHook.Keys.Control))
            {
                str.Append("Ctrl+");
            }
            if (e.Modifiers.HasFlag(WindowsHook.Keys.Shift))
            {
                str.Append("Shift+");
            }
            if (e.Modifiers.HasFlag(WindowsHook.Keys.Alt))
            {
                str.Append("Alt+");
            }
            if (e.Modifiers.HasFlag(WindowsHook.Keys.LWin) || e.Modifiers.HasFlag(WindowsHook.Keys.RWin))
            {
                str.Append("Win+");
            }

            if (e.KeyCode == WindowsHook.Keys.Oem3)
                str.Append("`");
            else
                str.Append(e.KeyCode);

            if (OnKeyDown != null)
            {
                str = str
                    .Replace("Back", "Backspace")
                    .Replace("Capital", "CapsLock")
                    .Replace("Next", "PageDown")
                    .Replace("Pause", "Break");

                bool result = true;

                result = OnKeyDown.Invoke(str.ToString());
                if (result)
                {
                    e.Handled = true;
                }
            }
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            OnReset();
            isDisposed = true;
            GC.SuppressFinalize(this);
        }

        public void OnReset()
        {
            if (hookProvider.Hook != null) // Hook will be null if auto update was successful
            {
                hookProvider.Hook.KeyDown -= Hook_KeyDown;
            }
        }



        public bool IsKeyPressed(string key)
        {
            switch (key)
            {
                case "Ctrl":
                    return Keyboard.IsKeyPressed(Keyboard.VirtualKeyStates.VK_CONTROL)
                               || Keyboard.IsKeyPressed(Keyboard.VirtualKeyStates.VK_LCONTROL)
                               || Keyboard.IsKeyPressed(Keyboard.VirtualKeyStates.VK_RCONTROL);

                case "Alt":
                    return Keyboard.IsKeyPressed(Keyboard.VirtualKeyStates.VK_MENU)
                        || Keyboard.IsKeyPressed(Keyboard.VirtualKeyStates.VK_LMENU)
                        || Keyboard.IsKeyPressed(Keyboard.VirtualKeyStates.VK_RMENU);

                default:
                    //logger.Warning("NativeKeyboard.IsKeyPressed - Unrecognized key - {key}", key);
                    return false;
            };
        }

        // Below added by Orich

        public void CtrlReset()
        {
            DateTime now = DateTime.Now;
            while (IsKeyPressed("Ctrl") && (DateTime.Now - now).Milliseconds < 250)
            {
                Thread.Sleep(50);
            }
            if (IsKeyPressed("Ctrl"))
            {
                CtrlUp();
            }
        }

        public void AltReset()
        {
            DateTime now = DateTime.Now;
            while (IsKeyPressed("Alt") && (DateTime.Now - now).Milliseconds < 250)
            {
                Thread.Sleep(50);
            }
            if (IsKeyPressed("Alt"))
            {
                AltUp();
            }
        }
        public void ShiftReset()
        {
            DateTime now = DateTime.Now;
            while (IsKeyPressed("Shift") && (DateTime.Now - now).Milliseconds < 250)
            {
                Thread.Sleep(50);
            }
            if (IsKeyPressed("Shift"))
            {
                ShiftUp();
            }
        }

        public void CtrlDown()
        {
            CtrlReset();

            while (!IsKeyPressed("Ctrl"))
            {
                Keyboard.KeyDown(Windows.System.VirtualKey.LeftControl);
                Thread.Sleep(15);
            }
        }
        public void CtrlUp()
        {
            while (IsKeyPressed("Ctrl"))
            {
                Keyboard.KeyUp(Windows.System.VirtualKey.LeftControl);
                Thread.Sleep(15);
            }
        }

        public void AltDown()
        {
            while (!IsKeyPressed("Alt"))
            {
                Keyboard.KeyDown(Windows.System.VirtualKey.Menu);
                Thread.Sleep(15);
            }
        }
        public void AltUp()
        {
            while (IsKeyPressed("Alt"))
            {
                Keyboard.KeyUp(Windows.System.VirtualKey.Menu);
                Thread.Sleep(15);
            }
        }
        public void ShiftUp()
        {
            while (IsKeyPressed("Shift"))
            {
                Keyboard.KeyUp(Windows.System.VirtualKey.LeftShift);
                Thread.Sleep(15);
            }
        }
        public void ShiftDown()
        {
            while (!IsKeyPressed("Shift"))
            {
                Keyboard.KeyDown(Windows.System.VirtualKey.LeftShift);
                Thread.Sleep(15);
            }
        }


    }
}
