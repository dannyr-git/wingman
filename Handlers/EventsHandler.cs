using Microsoft.UI.Xaml;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using wingman.Helpers;
using wingman.Interfaces;
using wingman.Natives;
using wingman.Natives.Helpers;
using wingman.Views;

namespace wingman.Handlers
{
    public class EventsHandler : IDisposable
    {
        private readonly IKeybindEvents events;
        private readonly IMicrophoneDeviceService micService;
        private readonly IOpenAIAPIService chatGPTService;
        private readonly IStdInService stdInService;
        private readonly ILocalSettings settingsService;
        private readonly MediaPlayer mediaPlayer;
        private readonly ConcurrentBag<ModalWindow> openWindows = new ConcurrentBag<ModalWindow>();
        private readonly Stopwatch stopwatch = new Stopwatch();
        private bool isDisposed;
        private bool isRecording;

        public EventHandler<bool> InferenceCallback;

        public EventsHandler(IKeybindEvents events, IMicrophoneDeviceService micService, IOpenAIAPIService chatGPTService, IStdInService stdInService, ILocalSettings settingsService)
        {
            this.events = events;
            this.micService = micService;
            this.chatGPTService = chatGPTService;
            this.stdInService = stdInService;
            this.settingsService = settingsService;
            this.mediaPlayer = new MediaPlayer();

            Initialize();
        }

        private void Initialize()
        {
            events.OnMainHotkey += Events_OnMainHotkey;
            events.OnMainHotkeyRelease += Events_OnMainHotkeyRelease;

            events.OnModalHotkey += Events_OnModalHotkey;
            events.OnModalHotkeyRelease += Events_OnModalHotkeyRelease;

            isDisposed = false;
            isRecording = false;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                events.OnMainHotkey -= Events_OnMainHotkey;
                events.OnMainHotkeyRelease -= Events_OnMainHotkeyRelease;

                events.OnModalHotkey -= Events_OnModalHotkey;
                events.OnModalHotkeyRelease -= Events_OnModalHotkeyRelease;

                foreach (var window in openWindows)
                {
                    try
                    {
                        window.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error closing window: {ex.Message}");
                    }
                }

                mediaPlayer.Dispose();

                isDisposed = true;
            }
        }

        private async Task PlayChime(string chime)
        {
            var uri = new Uri(AppDomain.CurrentDomain.BaseDirectory + $"Assets\\{chime}.aac");
            mediaPlayer.Source = MediaSource.CreateFromUri(uri);
            mediaPlayer.Play();
        }

        private async Task MouseWait(bool wait)
        {
            InferenceCallback?.Invoke(this, wait);
        }


        private async Task CreateModal(string content)
        {
            ModalWindow dialog = new ModalWindow(content);
            openWindows.Add(dialog);
            dialog.Closed += Dialog_Closed;
            dialog.Activate();
            dialog.SetWindowSize(640, 480);
            dialog.Title = "Wingman Codeblock";

        }

        private void Dialog_Closed(object sender, WindowEventArgs e)
        {
            if (sender is ModalWindow window && openWindows.Contains(window))
            {
                openWindows.TryTake(out window);
            }
        }

        private async Task<bool> HandleHotkey(Func<Task<bool>> action)
        {
            if (isDisposed)
            {
                return await Task.FromResult(false);
            }

            if (isRecording)
            {
                return await Task.FromResult(true);
            }
            stopwatch.Restart();
            await PlayChime("normalchime");
            await micService.StartRecording();
            isRecording = true;
            return await action();
        }

        private async Task<bool> HandleHotkeyRelease(Func<string, Task<bool>> action)
        {
            if (!isRecording)
            {
                return await Task.FromResult(false);
            }

            await MouseWait(true);
            await PlayChime("lowchime");
            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;

            var file = await micService.StopRecording();
            isRecording = false;

            if (elapsed.TotalSeconds < 2)
            {
                return await Task.FromResult(true);
            }

            if (file == null)
            {
                throw new Exception("File is null");
            }

            var prompt = await chatGPTService.GetWhisperResponse(file);
            if (String.IsNullOrEmpty(prompt))
            {
                return await Task.FromResult(true);
            }

            string? cbstr = "";

            if (settingsService.Load<bool>("Append_Clipboard"))
            {
                cbstr = await ClipboardHelper.GetTextAsync();
                if (!String.IsNullOrEmpty(cbstr))
                {
                    prompt = PromptCleaners.TrimWhitespaces(cbstr);
                    prompt = PromptCleaners.TrimNewlines(cbstr);
                    prompt += cbstr;
                }
            }

            await file.DeleteAsync();

            var response = await chatGPTService.GetResponse(prompt);

            if (String.IsNullOrEmpty(response))
                response = "There was an error getting a response from OpenAI.";

            await action(response);

            return await Task.FromResult(true);
        }

        private async Task<bool> Events_OnMainHotkey()
        {
            return await HandleHotkey(async () =>
            {
                // In case hotkeys end up being snowflakes
                return await Task.FromResult(true);
            });
        }

        private async Task<bool> Events_OnMainHotkeyRelease()
        {
            return await HandleHotkeyRelease(async (response) =>
            {
                await MouseWait(false);
                await stdInService.SendWithClipboardAsync(response);
                return await Task.FromResult(true);
            });
        }

        private async Task<bool> Events_OnModalHotkey()
        {
            return await HandleHotkey(async () =>
            {
                // In case hotkeys end up being snowflakes
                return await Task.FromResult(true);
            });
        }

        private async Task<bool> Events_OnModalHotkeyRelease()
        {
            return await HandleHotkeyRelease(async (response) =>
            {
                await MouseWait(false);
                await CreateModal(response);
                return await Task.FromResult(true);
            });
        }
    }



    /*public class EventsHandler
    {
        private readonly IKeybindEvents events;
        private readonly IMicrophoneDeviceService micService;
        private readonly IOpenAIAPIService chatGPTService;
        private readonly IStdInService stdInService;
        private readonly ILocalSettings settingsService;

        public EventHandler<bool> InferenceCallback;

        private static ConcurrentBag<ModalWindow> openWindows = new ConcurrentBag<ModalWindow>();

        private async Task CreateModal(string content)
        {
            ModalWindow dialog = new ModalWindow(content);
            openWindows.Add(dialog);
            dialog.Closed += Dialog_Closed;
            dialog.Activate();
            dialog.SetWindowSize(640, 480);
            dialog.Title = "Wingman Codeblock";
        }

        private static void Dialog_Closed(object sender, WindowEventArgs e)
        {
            if (sender is ModalWindow window && openWindows.Contains(window))
            {
                openWindows.TryTake(out window);
            }
        }

        public EventsHandler(IKeybindEvents events,
            IMicrophoneDeviceService micService,
            IOpenAIAPIService chatGPTService,
            IStdInService stdInService,
            ILocalSettings settingsService
            )
        {
            this.events = events;
            this.micService = micService;
            this.chatGPTService = chatGPTService;
            this.stdInService = stdInService;
            this.settingsService = settingsService;
            mediaPlayer = new MediaPlayer();

            Initialize();
        }

        // hacking this together
        private MediaPlayer mediaPlayer;

        private void PlayNormalChime()
        {
            //going to unpackaged broke literally everything ... everything requires a package identity
            //mediaPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/normalchime.aac"));
            var uri = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"Assets\\normalchime.aac");

            mediaPlayer.Source = MediaSource.CreateFromUri(uri);
            mediaPlayer.Play();
        }

        private void PlayHighChime()
        {
            var uri = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"Assets\\lowchime.aac");
            mediaPlayer.Source = MediaSource.CreateFromUri(uri);

            mediaPlayer.Play();
        }

        private async void MouseWait()
        {
            InferenceCallback?.Invoke(this, true);
        }

        private async void MouseArrow()
        {
            InferenceCallback?.Invoke(this, false);
        }

        private void Initialize()
        {
            events.OnMainHotkey += Events_OnMainHotkey;
            events.OnMainHotkeyRelease += Events_OnMainHotkeyRelease;

            events.OnModalHotkey += Events_OnModalHotkey;
            events.OnModalHotkeyRelease += Events_OnModalHotkeyRelease;
        }

        private Stopwatch stopwatch = new Stopwatch();
        bool isRecording = false;

        // Event handlers for OnMainHotKey
        #region OnMainHotKey
        private async Task<bool> Events_OnMainHotkey()
        {
            if (isRecording)
                return await Task.FromResult(true);

            stopwatch.Restart();
            PlayNormalChime();

            await micService.StartRecording();
            isRecording = true;

            return await Task.FromResult(true);
        }

        private async Task<bool> Events_OnMainHotkeyRelease()
        {
            while (!isRecording)
            {
                await Task.Delay(100);
            }
            // Stop the stopwatch and get the elapsed time
            stopwatch.Stop();
            PlayHighChime();

            var elapsed = stopwatch.Elapsed;

            // If at least 3 seconds haven't elapsed, return early
            if (elapsed.TotalSeconds < 2)
            {
                await micService.StopRecording();
                isRecording = false;
                return await Task.FromResult(true);
            }

            var file = await micService.StopRecording();

            if (file == null)
            {
                throw new Exception("File is null");
            }

            MouseWait();
            var prompt = await chatGPTService.GetWhisperResponse(file);
            // check settings for Append_Clipboard and if its true, append the clipboard to the prompt
            if (settingsService.Load<bool>("Append_Clipboard"))
            {
                var text = await ClipboardHelper.GetTextAsync();
                if (!string.IsNullOrEmpty(text))
                {
                    text = PromptCleaners.TrimWhitespaces(text);
                    text = PromptCleaners.TrimNewlines(text);
                    prompt += text;
                }
            }

            await file.DeleteAsync();
            string? resp;

            if (!string.IsNullOrEmpty(prompt))
            {
                resp = await chatGPTService.GetResponse(prompt);

                await stdInService.SendWithClipboardAsync(resp);
            }
            MouseArrow();

            isRecording = false;
            return await Task.FromResult(true);
        }
        #endregion



        // Event handlers for OnModalHotKey
        #region OnModalHotKey
        private async Task<bool> Events_OnModalHotkey()
        {
            if (isRecording)
                return await Task.FromResult(true);

            stopwatch.Restart();
            PlayNormalChime();

            await micService.StartRecording();
            isRecording = true;

            return await Task.FromResult(true);
        }

        private async Task<bool> Events_OnModalHotkeyRelease()
        {
            while (!isRecording)
            {
                await Task.Delay(100);
            }
            PlayHighChime();
            // Stop the stopwatch and get the elapsed time
            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;

            // If at least 3 seconds haven't elapsed, return early
            if (elapsed.TotalSeconds < 2)
            {
                await micService.StopRecording();
                isRecording = false;
                return await Task.FromResult(true);
            }

            var file = await micService.StopRecording();

            if (file == null)
            {
                throw new Exception("File is null");
            }

            MouseWait();
            var prompt = await chatGPTService.GetWhisperResponse(file);

            // check settings for Append_Clipboard_Modal and if its true, append the clipboard to the prompt
            if (settingsService.Load<bool>("Append_Clipboard_Modal"))
            {
                var text = await ClipboardHelper.GetTextAsync();
                if (!string.IsNullOrEmpty(text))
                {
                    text = PromptCleaners.TrimWhitespaces(text);
                    text = PromptCleaners.TrimNewlines(text);
                    prompt += text;
                }
            }

            await file.DeleteAsync();

            if (!string.IsNullOrEmpty(prompt))
            {
                var resp = await chatGPTService.GetResponse(prompt);

                if (!String.IsNullOrEmpty(resp))
                    await CreateModal(resp);

                ClipboardHelper.SetText(resp);
            }
            MouseArrow();

            isRecording = false;
            return await Task.FromResult(true);
        }
        #endregion

        public void Dispose()
        {
            events.OnMainHotkey -= Events_OnMainHotkey;
            events.OnMainHotkeyRelease -= Events_OnMainHotkeyRelease;

            events.OnModalHotkey -= Events_OnModalHotkey;
            events.OnModalHotkeyRelease -= Events_OnModalHotkeyRelease;

            foreach (var window in openWindows)
            {
                try
                {
                    window.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error closing window: {ex.Message}");
                }
            }

        }
    } */
}
