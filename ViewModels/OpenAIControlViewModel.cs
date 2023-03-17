using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Windows.Input;
using wingman.Interfaces;

namespace wingman.ViewModels
{
    public class OpenAIControlViewModel : ObservableObject
    {
        private readonly ILocalSettings _settingsService;
        private readonly IGlobalHotkeyService _globalHotkeyService;
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

        public OpenAIControlViewModel(ILocalSettings settingsService, IOpenAIAPIService openAIService, IGlobalHotkeyService globalHotkeyService)
        {
            _settingsService = settingsService;
            _globalHotkeyService = globalHotkeyService;

            Main_Hotkey_Toggled = false;
            Api_Key = _settingsService.Load<string>("ApiKey");
            Main_Hotkey = _settingsService.Load<string>("Main_Hotkey");
            Modal_Hotkey = _settingsService.Load<string>("Modal_Hotkey");
            Trim_Newlines = _settingsService.Load<bool>("Trim_Newlines");
            Trim_Whitespaces = _settingsService.Load<bool>("Trim_Whitespaces");
            Append_Clipboard = _settingsService.Load<bool>("Append_Clipboard");
            Append_Clipboard_Modal = _settingsService.Load<bool>("Append_Clipboard_Modal");
        }

        public bool Append_Clipboard_Modal
        {
            get => _appendclipboardmodal;
            set
            {
                _settingsService.Save<bool>("Append_Clipboard_Modal", value);
                SetProperty(ref _appendclipboardmodal, value);
            }
        }

        public bool Append_Clipboard
        {
            get => _appendclipboard;
            set
            {
                _settingsService.Save<bool>("Append_Clipboard", value);
                SetProperty(ref _appendclipboard, value);
            }
        }

        public bool Trim_Whitespaces
        {
            get => _trimwhitespaces;
            set
            {
                _settingsService.Save<bool>("Trim_Whitespaces", value);
                SetProperty(ref _trimwhitespaces, value);
            }
        }

        public bool Trim_Newlines
        {
            get => _trimnewlines;
            set
            {
                _settingsService.Save<bool>("Trim_Newlines", value);
                SetProperty(ref _trimnewlines, value);
            }
        }

        public string Main_Hotkey
        {
            get => _mainhotkey;
            set
            {
                _settingsService.Save("Main_Hotkey", value);  // TODO; actually manage hotkey key,value pair relationships
                SetProperty(ref _mainhotkey, value);
            }
        }

        public string Modal_Hotkey
        {
            get => _modalhotkey;
            set
            {
                _settingsService.Save("Modal_Hotkey", value);
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

            await _globalHotkeyService.ConfigureHotkeyAsync(NativeKeyboard_OnKeyDown);

            Main_Hotkey_Toggled = false;

            if (!String.IsNullOrEmpty(_purgatoryhotkey))
            {
                // try to clear sticky keys

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


        public bool IsValidKey()
        {
            return _isvalidkey;
        }

        bool _isvalidkey;
        public bool IsEnabled
        {
            get => _isvalidkey;
            set
            {
                SetProperty(ref _isvalidkey, value);
            }
        }

        private bool IsApiKeyValid()
        {
            if (String.IsNullOrEmpty(_apikey) || !_apikey.StartsWith("sk-") || _apikey.Length != 51)
                return false;
            return true;
        }

        private string _apikeymessage;
        public string ApiKeymessage
        {
            get
            {
                if (IsApiKeyValid())
                    return "Key format valid.";
                else
                    return "Invalid key format.";
            }
        }

        public string Api_Key
        {
            get => _apikey;
            set
            {
                SetProperty(ref _apikey, value);
                if (!IsApiKeyValid())
                {
                    _apikey = "You must enter a valid OpenAI API Key.";
                    IsEnabled = IsApiKeyValid();
                    OnPropertyChanged(nameof(Api_Key));
                }
                else
                {
                    _settingsService.TrySave("ApiKey", value);
                    IsEnabled = IsApiKeyValid();
                    Ioc.Default.GetRequiredService<IOpenAIAPIService>();
                }
                OnPropertyChanged(nameof(ApiKeymessage));
            }
        }
    }


}