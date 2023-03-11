using Microsoft.UI.Xaml;
using wingman.Interfaces;
using wingman.Views;

namespace wingman.Services
{
    public class AppActivationService : IAppActivationService
    {
        private readonly MainWindow _mainWindow;
        private readonly IWindowingService _windowingService;
        private readonly IAppTitleBarService _appTitleBarService;
        private readonly ISettingsService _settingsService;
        private readonly IAppThemeService _appThemeService;
        private readonly ILocalizationService _localizationService;

        public AppActivationService(
            MainWindow mainWindow,
            IWindowingService windowingService,
            IAppTitleBarService appTitleBarService,
            ISettingsService settingsService,
            IAppThemeService appThemeService,
            ILocalizationService localizationService)
        {
            _mainWindow = mainWindow;
            _windowingService = windowingService;
            _appTitleBarService = appTitleBarService;
            _settingsService = settingsService;
            _appThemeService = appThemeService;
            _localizationService = localizationService;
        }

        public void Activate(object activationArgs)
        {
            InitializeServices();
            _mainWindow.Activate();
        }

        private void InitializeServices()
        {
            _windowingService.Initialize(_mainWindow);

            _appTitleBarService.Initialize(_mainWindow.TitleBar);

            if (_mainWindow.Content is FrameworkElement rootElement)
            {
                _appThemeService.Initialize(rootElement);
                //_appThemeService.SetTheme(ElementTheme.Dark);
            }

            _localizationService.Initialize(); // TODO: Actually implement localization
        }
    }
}