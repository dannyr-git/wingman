using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Graphics;
using WinRT.Interop;

namespace wingman.Helpers
{
    public static class TaskExtensions
    {
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeout)))
                return await task;
            else
                throw new TimeoutException("The operation has timed out.");
        }
    }


    public static class UIElementExtensions
    {
        public static void ChangeCursor(this UIElement uiElement, InputCursor cursor)
        {
            Type type = typeof(UIElement);
            type.InvokeMember("ProtectedCursor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, uiElement, new object[] { cursor });
        }


    }

    public static class Extensions
    {
        public static void SetIcon(this Window window, string iconpath)
        {
            AppWindow appWindow = window.GetAppWindow();
            appWindow.SetIcon(iconpath);

            if (appWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {


            }
        }



        public static void HideTitleBar(this Window window)
        {
            AppWindow appWindow = window.GetAppWindow();
            appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            appWindow.TitleBar.ButtonForegroundColor = Microsoft.UI.Colors.Transparent;
            appWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
            appWindow.TitleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
            appWindow.TitleBar.ButtonHoverBackgroundColor = Microsoft.UI.Colors.Transparent;
            appWindow.TitleBar.ButtonPressedBackgroundColor = Microsoft.UI.Colors.Transparent;
            appWindow.TitleBar.ButtonInactiveForegroundColor = Microsoft.UI.Colors.Transparent;
            appWindow.TitleBar.ButtonHoverForegroundColor = Microsoft.UI.Colors.Transparent;
            appWindow.TitleBar.ButtonPressedForegroundColor = Microsoft.UI.Colors.Transparent;
            appWindow.TitleBar.ForegroundColor = Microsoft.UI.Colors.Transparent;
            appWindow.TitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
            appWindow.TitleBar.PreferredHeightOption = 0;

            if (appWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {
                overlappedPresenter.IsMinimizable = false;
                overlappedPresenter.IsMaximizable = false;
                overlappedPresenter.SetBorderAndTitleBar(false, false);
            }
        }

        public static AppWindow GetAppWindow(this Window window)
        {
            IntPtr windowHandle = WindowNative.GetWindowHandle(window);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            return AppWindow.GetFromWindowId(windowId);
        }

        public static (int Width, int Height) GetCurrentWindowSize(this Window window)
        {
            SizeInt32 size = window.GetAppWindow().Size;
            return (size.Width, size.Height);
        }

        public static void Hide(this Window window)
        {
            AppWindow appWindow = window.GetAppWindow();
            appWindow.Hide();
        }

        public static void SetWindowPosition(this Window window, int x, int y)
        {
            AppWindow appWindow = window.GetAppWindow();
            PointInt32 position = new(x, y);
            appWindow.Move(position);
        }

        public static void SetWindowSize(this Window window, int width, int height)
        {
            AppWindow appWindow = window.GetAppWindow();
            SizeInt32 size = new(width, height);
            appWindow.Resize(size);
        }

        public static void SetIsResizable(this Window window, bool value)
        {
            AppWindow appWindow = window.GetAppWindow();

            if (appWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {
                overlappedPresenter.IsResizable = value;
                overlappedPresenter.IsMaximizable = value;

                return;
            }

            throw new NotSupportedException($"Always on top is not supported with {appWindow.Presenter.Kind}.");
        }

        public static void SetNeverFocused(this Window window, bool value)
        {
            AppWindow appWindow = window.GetAppWindow();

            if (appWindow == null)
                return;

            if (appWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {

                return;
            }

            throw new NotSupportedException($"Cna't support never focused window {appWindow.Presenter.Kind}.");
        }

        public static void SetIsAlwaysOnTop(this Window window, bool value)
        {
            AppWindow appWindow = window.GetAppWindow();

            if (appWindow == null)
                return;

            if (appWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {
                overlappedPresenter.IsAlwaysOnTop = value;
                return;
            }

            throw new NotSupportedException($"Always on top is not supported with {appWindow.Presenter.Kind}.");
        }

        public static bool GetIsAlwaysOnTop(this Window window)
        {
            AppWindow appWindow = window.GetAppWindow();

            if (appWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {
                return overlappedPresenter.IsAlwaysOnTop;
            }

            throw new NotSupportedException($"Always on top is not supported with {appWindow.Presenter.Kind}.");
        }
    }
}