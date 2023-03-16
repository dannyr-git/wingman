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
                return;
            }

            throw new NotSupportedException($"Always on top is not supported with {appWindow.Presenter.Kind}.");
        }

        public static void SetIsAlwaysOnTop(this Window window, bool value)
        {
            AppWindow appWindow = window.GetAppWindow();

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