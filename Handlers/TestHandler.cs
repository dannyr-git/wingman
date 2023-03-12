using wingman.Interfaces;
using wingman.Natives;
using wingman.ViewModels;
using wingman.Views;

namespace wingman.Handlers
{
    public class TestHandler
    {
        private readonly EventsHandler _eventsHandler;
        private readonly HookProvider _hookProvider;
        private readonly INativeKeyboard _nativeKeyboard;
        private readonly IKeybindEvents _keybindEvents;
        private readonly IMicrophoneDeviceService _microphoneDeviceService;
        private readonly IEditorService _editorService;
        private readonly IStdInService _stdInService;
        private readonly ISettingsService _settingsService;
        private readonly IAppActivationService _appActivationService;
        private readonly IOpenAIAPIService _openAIAPIService;
        private readonly AudioInputControlViewModel _audioInputControlViewModel;
        private readonly OpenAIControlViewModel _openAIControlViewModel;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly ModalControlViewModel _modalControlViewModel;
        private readonly MainPageViewModel _mainPageViewModel;
        private readonly MainWindow _mainWindow;

        public TestHandler(
            EventsHandler eventsHandler,
            HookProvider hookProvider
        //,INativeKeyboard nativeKeyboard
        , IKeybindEvents keybindEvents
        //,IMicrophoneDeviceService microphoneDeviceService
        //,IEditorService editorService
        , IStdInService stdInService
        , ISettingsService settingsService
        //,IAppTitleBarService appTitleBarService
        //,/IAppActivationService appActivationService
        , IOpenAIAPIService openAIAPIService
        //,IWindowingService windowingService
        //,AudioInputControlViewModel audioInputControlViewModel
        //,OpenAIControlViewModel openAIControlViewModel
        //,MainWindowViewModel mainWindowViewModel
        //,ModalControlViewModel modalControlViewModel
        //,MainPageViewModel mainPageViewModel
        , MainWindow mainWindow
        )
        {
            //_eventsHandler = eventsHandler;
            _hookProvider = hookProvider;
            //_nativeKeyboard = nativeKeyboard;
            //_keybindEvents = keybindEvents;
            //_microphoneDeviceService = microphoneDeviceService;
            //_editorService = editorService;
            //_stdInService = stdInService;
            //_settingsService = settingsService;
            //_appThemeService = appThemeService;
            //_localizationService = localizationService;
            //_appTitleBarService = appTitleBarService;
            //_appActivationService = appActivationService;
            //_openAIAPIService = openAIAPIService;
            //_windowingService = windowingService;
            //_audioInputControlViewModel = audioInputControlViewModel;
            //_openAIControlViewModel = openAIControlViewModel;
            //_mainWindowViewModel = mainWindowViewModel;
            //_modalControlViewModel = modalControlViewModel;
            //_mainPageViewModel = mainPageViewModel;
            //_mainWindow = mainWindow;
        }
    }

}
