using System;
using System.Diagnostics;
using wingman.Interfaces;
using wingman.Views;

namespace wingman.Services
{
    public class AppActivationService : IAppActivationService, IDisposable
    {
        private readonly MainWindow _mainWindow;
        private readonly ISettingsService _settingsService;

        public AppActivationService(
            MainWindow mainWindow,
            ISettingsService settingsService)
        {
            _mainWindow = mainWindow;
            _settingsService = settingsService;
        }

        public void Activate(object activationArgs)
        {
            InitializeServices();
            _mainWindow.Activate();
        }

        public void Dispose()
        {
            Debug.WriteLine("Appactivate Disposed");
            //    _app.Dispose();
        }

        private void InitializeServices()
        {
        }
    }
}