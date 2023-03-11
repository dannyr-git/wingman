﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Input.Preview.Injection;
using wingman.Interfaces;
using wingman.Natives.Helpers;

namespace wingman.Services
{
    public class StdInService : IStdInService
    {
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        // import VkKeyScanA function from user32.dll
        [DllImport("user32.dll")]
        static extern short VkKeyScanA(char ch);

        const uint WM_CHAR = 0x0102;

        private Process? _process;

        public void SetProcess(Process process)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
        }

        ISettingsService settingsService;

        public StdInService(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }


        public async Task SendStringAsync(string str)
        {
            InputInjector inputInjector = InputInjector.TryCreate();
            // Save whatever is on the clipboard
            var savedClipboard = await ClipboardHelper.GetTextAsync();

            ClipboardHelper.SetText(str);


            // Paste the clipboard out
            inputInjector.InjectKeyboardInput(new[] {
                new InjectedInputKeyboardInfo
                {
            VirtualKey = (ushort) VirtualKey.Control,
            KeyOptions = InjectedInputKeyOptions.None
        },
        new InjectedInputKeyboardInfo
        {
            VirtualKey = (ushort) VirtualKey.V,
            KeyOptions = InjectedInputKeyOptions.None
        },
        new InjectedInputKeyboardInfo
        {
            VirtualKey = (ushort) VirtualKey.Control,
            KeyOptions = InjectedInputKeyOptions.KeyUp
        }
    });


            // Restore the original clipboard contents
            ClipboardHelper.SetText(savedClipboard);
        }

        // The new InputInjector class works 500x better than SendKeys; so we abandon NativeKeyboard here 

        /*
        public async Task SendStringAsync(string str)
        {
            InputInjector inputInjector = InputInjector.TryCreate();
            if (inputInjector == null)
            {
                throw new InvalidOperationException("Failed to create input injector.");
            }

            List<InjectedInputKeyboardInfo> inputBuffer = new List<InjectedInputKeyboardInfo>();

            foreach (char c in str)
            {
                var loworderbyte = new InjectedInputKeyboardInfo();

                short vk = VkKeyScanA(c);
                byte vkCode = (byte)(vk & 0xFF);
                loworderbyte.VirtualKey = (ushort)vkCode;

                if ((vk & 0x100) != 0)
                {
                    var highorderbyteup = new InjectedInputKeyboardInfo();
                    highorderbyteup.VirtualKey = (ushort)VirtualKey.Shift;
                    highorderbyteup.KeyOptions = InjectedInputKeyOptions.KeyUp;

                    var highorderbytedown = new InjectedInputKeyboardInfo();
                    highorderbytedown.VirtualKey = (ushort)VirtualKey.Shift;
                    highorderbytedown.KeyOptions = InjectedInputKeyOptions.None;

                    inputBuffer.Add(highorderbytedown);
                    inputBuffer.Add(loworderbyte);
                    inputBuffer.Add(highorderbyteup);
                    //inputInjector.InjectKeyboardInput(new[] { highorderbytedown, loworderbyte, highorderbyteup });
                }
                else
                {
                    loworderbyte.VirtualKey = (ushort)vkCode;
                    inputBuffer.Add(loworderbyte);
                    //inputInjector.InjectKeyboardInput(new[] { loworderbyte });
                }

            }


            // To speed up streaming the output, we'll batch keys sent with InjectKeyboardInput
            if (inputBuffer.Count > 0)
            {
                for (int i = 0; i < inputBuffer.Count;)
                {
                    var count = Math.Min(4, inputBuffer.Count - i); // This is randomly unstable again; 8 was the magic number but now I'm trying 4 to be safe
                    var plus1 = i + count + 1;
                    var plus2 = i + count + 2;
                    // We need to make sure Shift-Down and Shift-Up are sent together in the same batch or things get wonky
                    if (plus1 < inputBuffer.Count && inputBuffer[plus1].KeyOptions == InjectedInputKeyOptions.KeyUp)
                    {
                        count -= 2;
                    }
                    else if (plus2 < inputBuffer.Count && inputBuffer[plus2].KeyOptions == InjectedInputKeyOptions.KeyUp)
                    {
                        count--;
                    }
                    inputInjector.InjectKeyboardInput(inputBuffer.GetRange(i, count));
                    await Task.Delay(10); // Small delay required, either for injection method or for STDIN to register on the other end
                    i += count;
                }
            }


        }
        */


    }

}



