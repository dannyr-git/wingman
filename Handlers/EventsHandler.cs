using System;
using System.Diagnostics;
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
    public class EventsHandler
    {
        private readonly IKeybindEvents events;
        private readonly IMicrophoneDeviceService micService;
        private readonly IOpenAIAPIService chatGPTService;
        private readonly IStdInService stdInService;
        private readonly ISettingsService settingsService;
        //private readonly INamedPipesService namedPipesService;

        public EventHandler<bool> InferenceCallback;

        public EventsHandler(IKeybindEvents events,
            IMicrophoneDeviceService micService,
            IOpenAIAPIService chatGPTService,
            IStdInService stdInService,
            ISettingsService settingsService
            //INamedPipesService namedPipesService
            )
        {
            this.events = events;
            this.micService = micService;
            this.chatGPTService = chatGPTService;
            this.stdInService = stdInService;
            this.settingsService = settingsService;
            mediaPlayer = new MediaPlayer();
            //this.namedPipesService = namedPipesService;

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
            if (settingsService.Load<bool>("Wingman", "Append_Clipboard"))
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
            if (settingsService.Load<bool>("Wingman", "Append_Clipboard_Modal"))
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

                ModalWindow dialog = new ModalWindow(resp);
                dialog.Activate();
                dialog.SetWindowSize(640, 480);
                dialog.Title = "Wingman Codeblock";

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
        }
    }
}
