using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using wingman.Helpers;
using wingman.Interfaces;
using wingman.ViewModels;

namespace wingman.Services
{
    public class EventHandlerService : IEventHandlerService, IDisposable
    {
        private readonly IGlobalHotkeyService globalHotkeyService;
        private readonly IMicrophoneDeviceService micService;
        private readonly IStdInService stdInService;
        private readonly ISettingsService settingsService;
        private readonly ILoggingService Logger;
        private readonly IWindowingService windowingService;
        private readonly OpenAIControlViewModel openAIControlViewModel;
        private readonly MediaPlayer mediaPlayer;
        private readonly Stopwatch micQueueDebouncer = new Stopwatch();
        private bool isDisposed;
        private bool isRecording;
        private bool isProcessing;

        public EventHandler<bool> InferenceCallback { get; set; }

        public EventHandlerService(OpenAIControlViewModel openAIControlViewModel,
            IGlobalHotkeyService globalHotkeyService,
            IMicrophoneDeviceService micService,
            IStdInService stdInService,
            ISettingsService settingsService,
            ILoggingService loggingService,
            IWindowingService windowingService
            )
        {
            this.globalHotkeyService = globalHotkeyService;
            this.micService = micService;
            this.stdInService = stdInService;
            this.settingsService = settingsService;
            Logger = loggingService;
            this.windowingService = windowingService;
            mediaPlayer = new MediaPlayer();
            this.openAIControlViewModel = openAIControlViewModel;

            Initialize();
        }

        private void Initialize()
        {
            globalHotkeyService.RegisterHotkeyDown(WingmanSettings.Main_Hotkey, Events_OnMainHotkey);
            globalHotkeyService.RegisterHotkeyUp(WingmanSettings.Main_Hotkey, Events_OnMainHotkeyRelease);

            globalHotkeyService.RegisterHotkeyDown(WingmanSettings.Modal_Hotkey, Events_OnModalHotkey);
            globalHotkeyService.RegisterHotkeyUp(WingmanSettings.Modal_Hotkey, Events_OnModalHotkeyRelease);

            isDisposed = false;
            isRecording = false;
            isProcessing = false;

            Logger.LogDebug("EventHandler initialized.");
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                globalHotkeyService.UnregisterHotkeyDown(WingmanSettings.Main_Hotkey, Events_OnMainHotkey);
                globalHotkeyService.UnregisterHotkeyUp(WingmanSettings.Main_Hotkey, Events_OnMainHotkeyRelease);

                globalHotkeyService.UnregisterHotkeyDown(WingmanSettings.Modal_Hotkey, Events_OnModalHotkey);
                globalHotkeyService.UnregisterHotkeyUp(WingmanSettings.Modal_Hotkey, Events_OnModalHotkeyRelease);




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
#if DEBUG
            Logger.LogDebug("Hotkey Down Caught");
#else
            Logger.LogInfo("Recording has started ...");
#endif
            micQueueDebouncer.Restart();
            await PlayChime("normalchime");
            await micService.StartRecording();
            isRecording = true;
            return await action();
        }

        private async Task<bool> HandleHotkeyRelease(Func<string, Task<bool>> action, string callername)
        {
            if (!isRecording || isProcessing)
            {
                return await Task.FromResult(true);
            }

            try
            {
                Logger.LogDebug("Hotkey Up Caught");
                isProcessing = true;
                await MouseWait(true);
                await PlayChime("lowchime");
                micQueueDebouncer.Stop();
                var elapsed = micQueueDebouncer.Elapsed;

#if DEBUG
                Logger.LogDebug("Stop recording");
#else
                Logger.LogInfo("Stopping recording...");
#endif
                if (elapsed.TotalSeconds < 1)
                    await Task.Delay(1000);

                var file = await micService.StopRecording();
                await Task.Delay(100);
                isRecording = false;

                if (elapsed.TotalSeconds < 2)
                {
                    Logger.LogError("Recording was too short.");
                    return await Task.FromResult(true);
                }

                if (file == null)
                {
                    throw new Exception("File is null");
                }
#if DEBUG
Logger.LogDebug("Send recording to Whisper API");
#else
                Logger.LogInfo("Initiating Whisper API request...");
#endif
                windowingService.UpdateStatus("Waiting for Whisper API Response... (This can lag)");

                string prompt = string.Empty;
                var taskwatch = new Stopwatch();
                taskwatch.Start();
                using (var scope = Ioc.Default.CreateScope())
                {
                    var openAIAPIService = scope.ServiceProvider.GetRequiredService<IOpenAIAPIService>();
                    var whisperResponseTask = openAIAPIService.GetWhisperResponse(file);


                    while (!whisperResponseTask.IsCompleted)
                    {
                        await Task.Delay(50);
                        if (taskwatch.Elapsed.TotalSeconds >= 3)
                        {
                            taskwatch.Restart();
                            Logger.LogInfo("   Still waiting...");
                        }
                    }
                    prompt = await whisperResponseTask;
                }
                taskwatch.Stop();

                windowingService.UpdateStatus("Whisper API Responded...");
#if DEBUG
Logger.LogDebug("WhisperAPI Prompt Received: " + prompt);
#else
#endif
                if (string.IsNullOrEmpty(prompt))
                {
                    Logger.LogError("WhisperAPI Prompt was Empty");
                    return await Task.FromResult(true);
                }
                Logger.LogInfo("Whisper API responded: " + prompt);

                string? cbstr = "";

                if ((settingsService.Load<bool>(WingmanSettings.Append_Clipboard) && callername=="MAIN_HOTKEY") || (settingsService.Load<bool>(WingmanSettings.Append_Clipboard_Modal) && callername=="MODAL_HOTKEY"))
                {
#if DEBUG
                    Logger.LogDebug("WingmanSettings.Append_Clipboard is true.");
#else
                    Logger.LogInfo("Appending clipboard to prompt...");
#endif
                    cbstr = await ClipboardHelper.GetTextAsync();
                    if (!string.IsNullOrEmpty(cbstr))
                    {
                        cbstr = PromptCleaners.TrimWhitespaces(cbstr);
                        cbstr = PromptCleaners.TrimNewlines(cbstr);
                        prompt += " " + cbstr;
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

                string response = String.Empty;
                using (var scope = Ioc.Default.CreateScope())
                {
                    var openAIAPIService = scope.ServiceProvider.GetRequiredService<IOpenAIAPIService>();

                    try
                    {
                        windowingService.UpdateStatus("Waiting for GPT response...");
#if DEBUG
                    Logger.LogDebug("Sending prompt to OpenAI API: " + prompt);
#else
                        Logger.LogInfo("Waiting for GPT Response... (This can lag)");
#endif

                        var responseTask = openAIAPIService.GetResponse(prompt);
                        taskwatch = Stopwatch.StartNew();
                        while (!responseTask.IsCompleted)
                        {
                            await Task.Delay(50);
                            if (taskwatch.Elapsed.TotalSeconds >= 3)
                            {
                                taskwatch.Restart();
                                Logger.LogInfo("   Still waiting...");
                            }
                        }
                        response = await responseTask;
                        taskwatch.Stop();

                        windowingService.UpdateStatus("Response Received ...");
                        Logger.LogInfo("Received response from GPT...");
                    }
                    catch (Exception e)
                    {
                        Logger.LogException("Error sending prompt to OpenAI API: " + e.Message);
                        throw e;
                    }
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
#if DEBUG
                Logger.LogDebug("Returning");
#else
                Logger.LogInfo("Sending response to STDOUT...");
#endif
                await stdInService.SendWithClipboardAsync(response);
                return await Task.FromResult(true);
            }, "MAIN_HOTKEY");

            await MouseWait(false);
            micQueueDebouncer.Restart();
            isProcessing = false;
            windowingService.ForceStatusHide();
        }

        private async void Events_OnModalHotkey(object sender, EventArgs e)
        {
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
                Logger.LogInfo("Adding response to Clipboard...");
                await ClipboardHelper.SetTextAsync(response);
#if DEBUG
                Logger.LogDebug("Returning");
#else
                Logger.LogInfo("Sending response via Modal...");
#endif
                windowingService.ForceStatusHide();
                await Task.Delay(100); // make sure focus changes are done
                await windowingService.CreateModal(response);
                return await Task.FromResult(true);
            }, "MODAL_HOTKEY");

            await MouseWait(false);
            micQueueDebouncer.Restart();
            isProcessing = false;
        }
    }
}
