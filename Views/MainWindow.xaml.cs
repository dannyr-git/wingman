using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Core;
using wingman.Handlers;
using wingman.Helpers;
using wingman.ViewModels;

namespace wingman.Views
{
    public class GridExposeCursor : Grid
    {
        public InputCursor Cursor
        {
            get => ProtectedCursor;

            set => ProtectedCursor = value;
        }
    }

    public sealed partial class MainWindow : Window
    {
        public EventsHandler eventsHandler;
        private DispatcherQueue _dispatcherQueue;
        private App _app;

        public MainWindow(EventsHandler eventsHandler)
        {
            InitializeComponent();

            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            this.eventsHandler = eventsHandler;
            eventsHandler.InferenceCallback += HandleInferenceAsync;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(MainTitleBar);
            ViewModel = Ioc.Default.GetRequiredService<MainWindowViewModel>();

            this.SetWindowSize(800, 600);
            this.SetIsResizable(false);

            this.Closed += OnClosed;

            _app = null;
        }

        public void SetApp(App app)
        {
            this._app = app;
        }

        public MainWindowViewModel ViewModel { get; }

        private void OnClosed(object sender, WindowEventArgs e)
        {
            // Unsubscribe the event handler
            eventsHandler.InferenceCallback -= HandleInferenceAsync;

            // Remove the event handler for the Closed event
            ((Window)sender).Closed -= OnClosed;

            if (_app != null)
                _app.Dispose();
        }

        private async void HandleInferenceAsync(object sender, bool result)
        {
            // Your asynchronous code here
            if (result)
            {
                await CommunityToolkit.WinUI.DispatcherQueueExtensions.EnqueueAsync(_dispatcherQueue, () =>
                {
                    // Change the mouse cursor to waiting cursor

                    var cursor = InputCursor.CreateFromCoreCursor(new CoreCursor(CoreCursorType.Wait, 0));
                    this.MainGrid.Cursor = cursor;
                    //(ThisIsStupid as UIElement).ChangeCursor(InputSystemCursor.Create(InputSystemCursorShape.Wait));

                });
            }
            else
            {
                await CommunityToolkit.WinUI.DispatcherQueueExtensions.EnqueueAsync(_dispatcherQueue, () =>
                {
                    var cursor = InputCursor.CreateFromCoreCursor(new CoreCursor(CoreCursorType.Arrow, 0));
                    this.MainGrid.Cursor = cursor;

                    // Change the mouse cursor to arrow cursor
                    //(ThisIsStupid as UIElement).ChangeCursor(InputSystemCursor.Create(InputSystemCursorShape.Arrow));


                });
            }
        }



    }
}