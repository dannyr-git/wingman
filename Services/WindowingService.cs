using Microsoft.UI.Xaml;
using Windows.Graphics;
using wingman.Helpers;
using wingman.Interfaces;

namespace wingman.Services
{
    public class WindowingService : IWindowingService
    {
        private readonly ISettingsService _settingsService;
        private Window? _window;

        public WindowingService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            SettingsName = $"{GetType().Namespace}.{GetType().Name}";
        }

        public string SettingsName { get; set; }

        public string WindowWidthSettingsKey { get; set; } = "WindowWidth";

        public string WindowHeightSettingsKey { get; set; } = "WindowHeight";

        public string IsAlwaysOnTopSettingsKey { get; set; } = "IsAlwaysOnTop";

        public void Initialize(Window window)
        {
            _window = window;

            if (LoadWindowSizeSettings() is (int Width, int Height) && Width > 0 && Height > 0)
            {
                SetWindowSize(Width, Height);
            }
        }

        public (int Width, int Height)? LoadWindowSizeSettings()
        {
            if ((_settingsService.TryLoad(SettingsName, WindowWidthSettingsKey, out int width) is true &&
                (_settingsService.TryLoad(SettingsName, WindowHeightSettingsKey, out int height) is true)))
            {
                return (width, height);
            }

            return null;
        }

        public bool SaveWindowSizeSettings(int width, int height)
        {
            return
                _settingsService.TrySave(SettingsName, WindowWidthSettingsKey, width) &&
                _settingsService.TrySave(SettingsName, WindowHeightSettingsKey, height);
        }

        public (int Width, int Height)? GetWindowSize()
        {
            if (_window is not null)
            {
                SizeInt32 currentSize = _window.GetAppWindow().Size;
                return (currentSize.Width, currentSize.Height);
            }

            return null;
        }

        public void SetWindowSize(int width, int height)
        {
            if (_window is not null && width > 0 && height > 0)
            {
                _window.GetAppWindow().Resize(new SizeInt32(width, height));
            }
        }

        public bool? LoadIsAlwaysOnTopSettings()
        {
            if (_settingsService.TryLoad(SettingsName, IsAlwaysOnTopSettingsKey, out bool isAlwaysOnTop) is true)
            {
                return isAlwaysOnTop;
            }

            return null;
        }

        public bool SaveIsAlwaysOnTopSettings(bool isAlwaysOnTop)
        {
            return _settingsService.TrySave(SettingsName, IsAlwaysOnTopSettingsKey, isAlwaysOnTop);
        }

        public bool? GetIsAlwaysOnTop()
        {
            return _window?.GetIsAlwaysOnTop();
        }

        public void SetIsAlwaysOnTop(bool isAlwaysOnTop)
        {
            _window?.SetIsAlwaysOnTop(isAlwaysOnTop);
        }
    }
}