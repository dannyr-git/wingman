using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using wingman.Interfaces;

namespace wingman.ViewModels
{
    public class FooterViewModel : ObservableObject, IDisposable
    {
        private readonly ISettingsService _settingsService;
        private readonly ILoggingService _loggingService;

        private readonly DispatcherQueue _dispatcherQueue;

        private readonly EventHandler<string> LoggingService_OnLogEntry;
        private string _logText = "";
        private bool _disposed = false;
        private bool _disposing = false;

        public FooterViewModel(ISettingsService settingsService, ILoggingService loggingService)
        {
            _settingsService = settingsService;
            _loggingService = loggingService;

            try
            {
                _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't get dispatcherQueue: {ex.Message}");
            }

            LoggingService_OnLogEntry = async (sender, logEntry) =>
            {
                await LogHandler(logEntry);
            };

            _loggingService.UIOutputHandler += LoggingService_OnLogEntry;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            Debug.WriteLine("Footerview Disposed");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposing = true;
            _loggingService.UIOutputHandler -= LoggingService_OnLogEntry;
            _disposing = false;
            _disposed=true;
        }



        public string LogText
        {
            get => _logText;
            set
            {
                SetProperty(ref _logText, value);
            }
        }

        private async Task LogHandler(string logBook)
        {
            await _dispatcherQueue.EnqueueAsync(async () =>
            {
                LogText = logBook;
            });
        }

    }
}