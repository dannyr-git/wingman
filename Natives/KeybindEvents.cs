using System;
using System.Threading.Tasks;
using Windows.UI.Input.Preview.Injection;
using wingman.Interfaces;
using wingman.Natives.Helpers;

namespace wingman.Natives
{
    public class KeybindEvents : IKeybindEvents
    {
        private readonly INativeKeyboard nativeKeyboard;
        private readonly HookProvider hookProvider;
        private readonly ISettingsService settingsService;
        InputInjector inputInjector;

        public KeybindEvents(INativeKeyboard nativeKeyboard,
            HookProvider hookProvider,
            ISettingsService settingsService
            )
        {
            this.nativeKeyboard = nativeKeyboard;
            this.hookProvider = hookProvider;
            this.settingsService = settingsService;

            nativeKeyboard.OnKeyDown += NativeKeyboard_OnKeyDown;
            nativeKeyboard.OnKeyUp += NativeKeyboard_OnKeyUp;

            inputInjector = InputInjector.TryCreate();

            Enabled = true;
        }

        public bool Enabled { get; set; }
        private bool _toggled = true;

        public void ToggleKeybinds()
        {
            if (_toggled)
            {
                nativeKeyboard.OnKeyUp -= NativeKeyboard_OnKeyUp;
                nativeKeyboard.OnKeyDown -= NativeKeyboard_OnKeyDown;
            }
            else
            {
                nativeKeyboard.OnKeyUp += NativeKeyboard_OnKeyUp;
                nativeKeyboard.OnKeyDown += NativeKeyboard_OnKeyDown;
            }
            _toggled = !_toggled;
        }

        public event Func<Task<bool>> OnMainHotkey;
        public event Func<Task<bool>> OnMainHotkeyRelease;

        public event Func<Task<bool>> OnModalHotkey;
        public event Func<Task<bool>> OnModalHotkeyRelease;


        private bool NativeKeyboard_OnKeyUp(string input)
        {
            Task<bool> task = null;

            ExecuteKeybind("Main Hotkey", settingsService.Load<string>("Wingman", "Main_Hotkey"), input, OnMainHotkeyRelease, ref task);
            ExecuteKeybind("Modal Hotkey", settingsService.Load<string>("Wingman", "Modal_Hotkey"), input, OnModalHotkeyRelease, ref task);

            if (task == null)
            {
                Enabled = true;
            }
            else
            {
                Task.Run(async () =>
                {
                    await task;
                    Enabled = true;
                });
            }

            return task != null;
        }

        private bool NativeKeyboard_OnKeyDown(string input)
        {
            Task<bool> task = null;

            ExecuteKeybind("Main Hotkey", settingsService.Load<string>("Wingman", "Main_Hotkey"), input, OnMainHotkey, ref task);
            ExecuteKeybind("Modal Hotkey", settingsService.Load<string>("Wingman", "Modal_Hotkey"), input, OnModalHotkey, ref task);

            if (task == null)
            {
                Enabled = true;
            }
            else
            {
                Task.Run(async () =>
                {
                    await task;
                    Enabled = true;
                });
            }

            return task != null;
        }

        private void ExecuteKeybind(string name, string keybind, string input, Func<Task<bool>> func, ref Task<bool> returnTask)
        {
            if (input == keybind)
            {
                if (func != null)
                {
                    returnTask = func.Invoke();
                }


                //SendInputIf(keybind, input, returnTask);
            }
        }

        private void SendInputIf(string keybind, string input, Task<bool> task)
        {
            if (task != null && input == keybind)
            {
                Task.Run(async () =>
                {
                    if (!await task)
                    {
                        Enabled = false;
                        inputInjector.InjectKeyboardInput(KeyConverter.NormalizeKeystroke(keybind));
                        await Task.Delay(200);
                        Enabled = true;
                    }
                });
            }
        }
    }
}
