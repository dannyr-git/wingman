using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using wingman.Helpers;
using wingman.Interfaces;
using wingman.Natives.Helpers;
using wingman.ViewModels;

namespace wingman.Handlers
{
    public class EventsHandler : IDisposable
    {
        private readonly IGlobalHotkeyService globalHotkeyService;
        private readonly IMicrophoneDeviceService micService;
        private readonly IOpenAIAPIService chatGPTService;
        private readonly IStdInService stdInService;
        private readonly ILocalSettings settingsService;
        private readonly ILoggingService Logger;
        private readonly IWindowingService windowingService;
        private readonly OpenAIControlViewModel openAIControlViewModel;
        private readonly MediaPlayer mediaPlayer;
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly Stopwatch micQueueDebouncer = new Stopwatch();
        private bool isDisposed;
        private bool isRecording;
        private bool isProcessing;

        public EventHandler<bool> InferenceCallback;

        public EventsHandler(OpenAIControlViewModel openAIControlViewModel,
            IGlobalHotkeyService globalHotkeyService,
            IMicrophoneDeviceService micService,
            IOpenAIAPIService chatGPTService,
            IStdInService stdInService,
            ILocalSettings settingsService,
            ILoggingService loggingService,
            IWindowingService windowingService
            )
        {
            this.globalHotkeyService = globalHotkeyService;
            this.micService = micService;
            this.chatGPTService = chatGPTService;
            this.stdInService = stdInService;
            this.settingsService = settingsService;
            this.Logger = loggingService;
            this.windowingService = windowingService;
            this.mediaPlayer = new MediaPlayer();
            this.openAIControlViewModel = openAIControlViewModel;

            Initialize();
        }

        private void Initialize()
        {
            globalHotkeyService.RegisterHotkeyDown("Main_Hotkey", Events_OnMainHotkey);
            globalHotkeyService.RegisterHotkeyUp("Main_Hotkey", Events_OnMainHotkeyRelease);

            globalHotkeyService.RegisterHotkeyDown("Modal_Hotkey", Events_OnModalHotkey);
            globalHotkeyService.RegisterHotkeyUp("Modal_Hotkey", Events_OnModalHotkeyRelease);

            isDisposed = false;
            isRecording = false;
            isProcessing = false;

            Logger.LogDebug("EventHandler initialized.");
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                globalHotkeyService.UnregisterHotkeyDown("Main_Hotkey", Events_OnMainHotkey);
                globalHotkeyService.UnregisterHotkeyUp("Main_Hotkey", Events_OnMainHotkeyRelease);

                globalHotkeyService.UnregisterHotkeyDown("Modal_Hotkey", Events_OnModalHotkey);
                globalHotkeyService.UnregisterHotkeyUp("Modal_Hotkey", Events_OnModalHotkeyRelease);




                mediaPlayer.Dispose();
                Debug.WriteLine("EventHandler disposed.");

                isDisposed = true;
            }
        }

        private async Task PlayChime(string chime)
        {
            var uri = new Uri(AppDomain.CurrentDomain.BaseDirectory + $"Assets\\{chime}.aac");
            mediaPlayer.Source = MediaSource.CreateFromUri(uri);
            mediaPlayer.Play();
            Logger.LogDebug("Chime played.");
        }

        private async Task MouseWait(bool wait)
        {
            InferenceCallback?.Invoke(this, wait);
        }

        private async Task<bool> HandleHotkey(Func<Task<bool>> action)
        {
            if (isDisposed || !openAIControlViewModel.IsValidKey())
            {
                return await Task.FromResult(false);
            }

            if (isRecording || isProcessing || micQueueDebouncer.IsRunning && micQueueDebouncer.Elapsed.TotalSeconds < 1)
            {
                return await Task.FromResult(true);
            }
            Logger.LogDebug("Hotkey Down Caught");
            stopwatch.Restart();
            await PlayChime("normalchime");
            await micService.StartRecording();
            isRecording = true;
            return await action();
        }

        private async Task<bool> HandleHotkeyRelease(Func<string, Task<bool>> action)
        {
            if (!isRecording || isProcessing)
            {
                return await Task.FromResult(false);
            }

            try
            {
                Logger.LogDebug("Hotkey Up Caught");
                isProcessing = true;
                await MouseWait(true);
                await PlayChime("lowchime");
                stopwatch.Stop();
                var elapsed = stopwatch.Elapsed;

                await Task.Delay(500);
                Logger.LogDebug("Stop recording");
                var file = await micService.StopRecording();
                await Task.Delay(250);
                isRecording = false;

                if (elapsed.TotalSeconds < 2)
                {
                    return await Task.FromResult(true);
                }

                if (file == null)
                {
                    throw new Exception("File is null");
                }

                Logger.LogDebug("Send recording to Whisper API");
                var prompt = await chatGPTService.GetWhisperResponse(file);
                Logger.LogDebug("WhisperAPI Prompt Received: " + prompt);

                if (String.IsNullOrEmpty(prompt))
                {
                    Logger.LogError("WhisperAPI Prompt was Empty");
                    return await Task.FromResult(true);
                }

                string? cbstr = "";

                if (settingsService.Load<bool>("Append_Clipboard"))
                {
                    Logger.LogDebug("Append_Clipboard is true.");
                    cbstr = await ClipboardHelper.GetTextAsync();
                    if (!String.IsNullOrEmpty(cbstr))
                    {
                        prompt = PromptCleaners.TrimWhitespaces(cbstr);
                        prompt = PromptCleaners.TrimNewlines(cbstr);
                        prompt += cbstr;
                    }
                }

                try
                {
                    Logger.LogDebug("Deleting temporary voice file: " + file.Path);
                    await file.DeleteAsync();
                }
                catch (Exception e)
                {
                    Logger.LogException("Error deleting temporary voice file: " + e.Message);
                    throw e;
                }

                string response;
                try
                {
                    Logger.LogDebug("Sending prompt to OpenAI API: " + prompt);
                    response = await chatGPTService.GetResponse(prompt);
                }
                catch (Exception e)
                {
                    Logger.LogException("Error sending prompt to OpenAI API: " + e.Message);
                    throw e;
                }

                await action(response);
            }
            catch (Exception e)
            {
                Logger.LogException("Error handling hotkey release: " + e.Message);
                throw e;
            }

            return await Task.FromResult(true);
        }


        //private async Task<bool> Events_OnMainHotkey()
        private async void Events_OnMainHotkey(object sender, EventArgs e)
        {
            // return
            await HandleHotkey(async () =>
            {
                // In case hotkeys end up being snowflakes
                return await Task.FromResult(true);
            });
        }

        //private async Task<bool> Events_OnMainHotkeyRelease()
        private async void Events_OnMainHotkeyRelease(object sender, EventArgs e)
        {
            // return
            await HandleHotkeyRelease(async (response) =>
            {
                Logger.LogDebug("Returning");
                await MouseWait(false);
                await stdInService.SendWithClipboardAsync(response);
                micQueueDebouncer.Restart();
                isProcessing = false;
                return await Task.FromResult(true);
            });
        }

        //private async Task<bool> Events_OnModalHotkey()
        private async void Events_OnModalHotkey(object sender, EventArgs e)
        {
            // return
            await HandleHotkey(async () =>
            {
                // In case hotkeys end up being snowflakes
                return await Task.FromResult(true);
            });
        }

        //private async Task<bool> Events_OnModalHotkeyRelease()
        private async void Events_OnModalHotkeyRelease(object sender, EventArgs e)
        {
            //return
            await HandleHotkeyRelease(async (response) =>
            {
                await MouseWait(false);
                await windowingService.CreateModal(response);
                micQueueDebouncer.Restart();
                isProcessing = false;
                return await Task.FromResult(true);
            });
        }
    }
}
