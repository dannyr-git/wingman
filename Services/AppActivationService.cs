using wingman.Interfaces;
using wingman.Views;

namespace wingman.Services
{
    public class AppActivationService : IAppActivationService
    {
        private readonly MainWindow _mainWindow;
        private readonly ILocalSettings _settingsService;

        public AppActivationService(
            MainWindow mainWindow,
            ILocalSettings settingsService)
        {
            _mainWindow = mainWindow;
            _settingsService = settingsService;
        }

        public void Activate(object activationArgs)
        {
            InitializeServices();
            _mainWindow.Activate();
        }

        private void InitializeServices()
        {
        }
    }
}