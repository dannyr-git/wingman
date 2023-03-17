using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using wingman.Views;

namespace wingman.Interfaces
{
    public class WindowingService : IWindowingService, IDisposable
    {
        private readonly List<ModalWindow> openWindows = new List<ModalWindow>();
        ILoggingService Logger;
        private bool _disposed = false;
        private bool _disposing = false;

        public WindowingService(
            ILoggingService logger)
        {
            Logger = logger;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (var window in openWindows)
                    {
                        window.Closed -= Dialog_Closed;
                        window.Close();
                    }
                    _disposed = true;
                }
            }
        }

        public void Dispose()
        {
            _disposing = true;
            Dispose(_disposing);
            GC.SuppressFinalize(this);
            Debug.WriteLine("WindowingService disposed");
        }

        public async Task CreateCodeModal(string content)
        {
            ModalWindow dialog = new ModalWindow(content, 640, 480);
            dialog.Closed += Dialog_Closed;
            dialog.Title = "Wingman Codeblock";
            dialog.Activate();

            openWindows.Add(dialog);
            Logger.LogDebug("Modal activated.");
        }

        public async Task CreateModal(string content, string title = "Modal", int width = 640, int height = 480, bool isresizable = true, bool activated = true)
        {
            ModalWindow dialog = new ModalWindow(content, width, height, isresizable);

            dialog.Closed += Dialog_Closed;
            dialog.Title = title;

            if (activated)
                dialog.Activate();

            openWindows.Add(dialog);
            Logger.LogDebug("Modal activated.");
        }

        private void Dialog_Closed(object sender, WindowEventArgs e)
        {
            if (sender is ModalWindow window && openWindows.Contains(window))
            {
                openWindows.Remove(window);
            }
            Logger.LogDebug("Modal closed.");
        }

        public event EventHandler<string>? EventStatusChanged;
        public event EventHandler? EventForceStatusHide;

        public void ForceStatusHide()
        {
            if (EventForceStatusHide != null && !_disposed && !_disposing)
            {
                EventForceStatusHide(this, EventArgs.Empty);
            }
        }

        public void UpdateStatus(string currentStatus)
        {
            if (EventStatusChanged != null && !_disposed && !_disposing)
                EventStatusChanged?.Invoke(this, currentStatus);
        }


    }
}
