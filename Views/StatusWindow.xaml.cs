using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using wingman.Helpers;
using wingman.Interfaces;

namespace wingman.Views
{
    public sealed partial class StatusWindow : Window, INotifyPropertyChanged, IDisposable
    {
        private readonly IWindowingService _windowingService;
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly EventHandler<string> WindowingService_StatusChanged;
        private readonly EventHandler WindowingService_ForceStatusHide;
        private CancellationTokenSource _timerCancellationTokenSource;
        private readonly DispatcherQueue _dispatcher;
        private Window _previousActiveWindow;
        private const int StatusDisplayDurationMilliseconds = 20000;
        private bool _disposed = false;
        private bool _disposing = false;
        private bool _isVisible = false;

        public StatusWindow(IWindowingService windowingService)
        {
            _dispatcher = DispatcherQueue.GetForCurrentThread();
            _windowingService = windowingService;

            WindowingService_StatusChanged = async (sender, status) =>
            {
                await StatusChanged(status);
            };

            _windowingService.EventStatusChanged += WindowingService_StatusChanged;

            WindowingService_ForceStatusHide = async (sender, e) =>
            {
                _isVisible = false;
                await ForceStatusHide();
            };

            _windowingService.EventForceStatusHide += WindowingService_ForceStatusHide;


            _timerCancellationTokenSource = new CancellationTokenSource();

            InitializeComponent();

            this.Activated += StatusWindow_Loaded;

            this.SetTitleBar(null);
            this.HideTitleBar();
            this.SetIsResizable(false);

        }

        private string _currentStatus = string.Empty;

        public string CurrentStatus
        {
            get => _currentStatus;
            set
            {
                _currentStatus = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentStatus)));
            }
        }

        private void ResizeWindowToContent()
        {
            StatusTextBlock.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
            Windows.Foundation.Size desiredSize = StatusTextBlock.DesiredSize;

            int width = (int)Math.Ceiling(desiredSize.Width);
            int height = (int)Math.Ceiling(desiredSize.Height);
            this.SetWindowSize(width, height);
        }

        private void StatusWindow_Loaded(object sender, WindowActivatedEventArgs e)
        {
            _previousActiveWindow = Window.Current; // Store the previously active window
            _isVisible = true;
            this.SetIsAlwaysOnTop(true);
            UpdateWindowPosition();
            RootGrid.PointerPressed += StatusWindow_PointerPressed;


            // Immediately return focus to the previously active window
            if (_previousActiveWindow != null)
            {
                _previousActiveWindow.Activate();
            }
        }

        private void StatusWindow_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isVisible = false;
            this.Hide();
        }

        private async void UpdateWindowPosition()
        {
            this.SetWindowPosition(0, 0);
        }

        private async Task ForceStatusHide()
        {
            _isVisible = false;
            await _dispatcher.EnqueueAsync(() => this.Hide());
        }

        private async Task StatusChanged(string status)
        {
            await _dispatcher.EnqueueAsync(async () =>
            {
                if (_previousActiveWindow == null && this != Window.Current)
                    _previousActiveWindow = Window.Current;

                if (this == Window.Current && _previousActiveWindow != this)
                    _previousActiveWindow.Activate();

                _timerCancellationTokenSource.Cancel();
                _timerCancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = _timerCancellationTokenSource.Token;

                CurrentStatus = status;

                if (!_isVisible)
                    this.Activate();



                ResizeWindowToContent();
                UpdateWindowPosition();

                await ManageTimerAsync(token);
            });
        }


        private async Task ManageTimerAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(StatusDisplayDurationMilliseconds, token);
                await _dispatcher.EnqueueAsync(() => { _isVisible=false; this.Hide(); });
            }
            catch (TaskCanceledException)
            {
                // Do nothing, timer was reset
            }
            catch (Exception ex)
            {
                // Handle other unexpected exceptions here
                Debug.WriteLine($"ManageTimerAsync Exception: {ex.Message}");
            }
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    RootGrid.PointerPressed -= StatusWindow_PointerPressed;
                    _windowingService.EventStatusChanged -= WindowingService_StatusChanged;
                    _windowingService.EventForceStatusHide -= WindowingService_ForceStatusHide;
                    _timerCancellationTokenSource?.Cancel();
                    _timerCancellationTokenSource?.Dispose();
                    this.Close();
                    _disposed = true;
                    Debug.WriteLine("StatusWindow Disposed");

                }
            }
        }

        public void Dispose()
        {
            _disposing = true;
            Dispose(_disposing);
            GC.SuppressFinalize(this);
            Debug.WriteLine("StatusWindow Disposed");
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the DataContext of the RootGrid to the current instance
            RootGrid.DataContext = this;
        }


    }
}

