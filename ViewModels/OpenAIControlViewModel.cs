using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using wingman.Interfaces;
using wingman.Natives;

namespace wingman.ViewModels
{
    public class OpenAIControlViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly IOpenAIAPIService _openAIService;
        private readonly INativeKeyboard _nativeKeyboard;
        private readonly IKeybindEvents _keybindEvents;
        private bool _saveButtonEnabled;
        public ICommand ApplyKeyAndRestartCommand { get; private set; }
        private string _apikey;
        private bool _keypressed;

        private string _mainhotkey;
        private string _modalhotkey;
        private string _purgatoryhotkey;

        private bool _trimwhitespaces;
        private bool _trimnewlines;
        private bool _appendclipboard;
        private bool _appendclipboardmodal;

        public OpenAIControlViewModel(ISettingsService settingsService, IOpenAIAPIService openAIService, INativeKeyboard nativeKeyboard, IKeybindEvents keybindEvents)
        {
            _settingsService = settingsService;
            _openAIService = openAIService;
            _nativeKeyboard = nativeKeyboard;
            _keybindEvents = keybindEvents;

            SaveButtonEnabled = true;
            IsEnabled = CanExecuteApplyKeyAndRestartCommand();
            ApplyKeyAndRestartCommand = new RelayCommand(ExecuteApplyKeyAndRestartCommand, CanExecuteApplyKeyAndRestartCommand);

            Main_Hotkey_Toggled = false;
            Api_Key = _settingsService.Load<string>("OpenAI", "ApiKey");
            Main_Hotkey = _settingsService.Load<string>("Wingman", "Main_Hotkey");
            Modal_Hotkey = _settingsService.Load<string>("Wingman", "Modal_Hotkey");
            Trim_Newlines = _settingsService.Load<bool>("Wingman", "Trim_Newlines");
            Trim_Whitespaces = _settingsService.Load<bool>("Wingman", "Trim_Whitespaces");
            Append_Clipboard = _settingsService.Load<bool>("Wingman", "Append_Clipboard");
            Append_Clipboard_Modal = _settingsService.Load<bool>("Wingman", "Append_Clipboard_Modal");
        }

        public bool Append_Clipboard_Modal
        {
            get => _appendclipboardmodal;
            set
            {
                _settingsService.Save<bool>("Wingman", "Append_Clipboard_Modal", value);
                SetProperty(ref _appendclipboardmodal, value);
            }
        }

        public bool Append_Clipboard
        {
            get => _appendclipboard;
            set
            {
                _settingsService.Save<bool>("Wingman", "Append_Clipboard", value);
                SetProperty(ref _appendclipboard, value);
            }
        }

        public bool Trim_Whitespaces
        {
            get => _trimwhitespaces;
            set
            {
                _settingsService.Save<bool>("Wingman", "Trim_Whitespaces", value);
                SetProperty(ref _trimwhitespaces, value);
            }
        }

        public bool Trim_Newlines
        {
            get => _trimnewlines;
            set
            {
                _settingsService.Save<bool>("Wingman", "Trim_Newlines", value);
                SetProperty(ref _trimnewlines, value);
            }
        }

        public string Main_Hotkey
        {
            get => _mainhotkey;
            set
            {
                _settingsService.Save("Wingman", "Main_Hotkey", value);  // TODO; actually manage hotkey key,value pair relationships
                SetProperty(ref _mainhotkey, value);
            }
        }

        public string Modal_Hotkey
        {
            get => _modalhotkey;
            set
            {
                _settingsService.Save("Wingman", "Modal_Hotkey", value);
                SetProperty(ref _modalhotkey, value);
            }
        }

        public bool Main_Hotkey_Toggled
        {
            get => _keypressed;
            set => SetProperty(ref _keypressed, value);
        }

        public async void ConfigureHotkeyCommand(object sender, RoutedEventArgs e)
        {
            Main_Hotkey_Toggled = true;

            _keybindEvents.ToggleKeybinds();
            _nativeKeyboard.OnKeyDown += NativeKeyboard_OnKeyDown;

            while (Main_Hotkey_Toggled)
                await Task.Delay(500);

            _nativeKeyboard.OnKeyDown -= NativeKeyboard_OnKeyDown;
            _keybindEvents.ToggleKeybinds();

            if (!String.IsNullOrEmpty(_purgatoryhotkey))
            {
                // try to clear sticky keys
                if (_purgatoryhotkey.StartsWith("Ctrl"))
                    _nativeKeyboard.CtrlReset();
                else if (_purgatoryhotkey.StartsWith("Alt"))
                    _nativeKeyboard.AltReset();
                else if (_purgatoryhotkey.StartsWith("Shift"))
                    _nativeKeyboard.ShiftReset();

                if ((sender as ToggleButton).Name == "ConfigMainHotkeyButton")
                {
                    Main_Hotkey = _purgatoryhotkey;
                }
                else if ((sender as ToggleButton).Name == "ConfigModalHotkeyButton")
                {
                    Modal_Hotkey = _purgatoryhotkey;
                }

            }
        }

        private bool NativeKeyboard_OnKeyDown(string input)
        {
            if (input == "Escape" || String.IsNullOrEmpty(input))
            {
                _purgatoryhotkey = "";
                return true;
            }

            Main_Hotkey_Toggled = false;
            _purgatoryhotkey = input;
            return true;
        }


        public bool SaveButtonEnabled
        {
            get => _saveButtonEnabled;
            set
            {
                if (SetProperty(ref _saveButtonEnabled, value))
                {

                }
            }
        }

        private async void ExecuteApplyKeyAndRestartCommand()
        {
            AppRestartFailureReason restartError = AppInstance.Restart("");

            switch (restartError)
            {
                case AppRestartFailureReason.RestartPending:
                    //SendToast("Another restart is currently pending.");
                    break;
                case AppRestartFailureReason.InvalidUser:
                    //SendToast("Current user is not signed in or not a valid user.");
                    break;
                case AppRestartFailureReason.Other:
                    //SendToast("Failure restarting.");
                    break;
            }

            // Close the application
            Application.Current.Exit();
        }

        bool _isvalidkey;
        public bool IsEnabled
        {
            get => _isvalidkey;
            set
            {
                SetProperty(ref _isvalidkey, value);
                OnPropertyChanged(nameof(ApplyKeyAndRestartCommand));
            }
        }

        private bool CanExecuteApplyKeyAndRestartCommand()
        {
            if (_apikey == null || !_apikey.StartsWith("sk-") || _apikey.Length != 51)
                return false;
            return true;
        }

        public string Api_Key
        {
            get => _apikey;
            set
            {
                SetProperty(ref _apikey, value);
                _settingsService.TrySave("OpenAI", "ApiKey", value);
                IsEnabled = CanExecuteApplyKeyAndRestartCommand();
            }
        }
    }


}