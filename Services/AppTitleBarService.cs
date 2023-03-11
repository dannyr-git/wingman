using Microsoft.UI.Xaml.Media;
using Windows.UI;
using wingman.Interfaces;
using wingman.Views;

namespace wingman.Services
{
    public class AppTitleBarService : IAppTitleBarService
    {
        private readonly ISettingsService _settingsService;
        private AppTitleBar? _appTitleBar;

        public AppTitleBarService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            SettingsName = $"{GetType().Namespace}.{GetType().Name}";
        }

        public string SettingsName { get; set; }

        public string TextSettingsKey { get; set; } = "Text";

        public string BackgroundSettingsKey { get; set; } = "Background";

        public string ForegroundSettingsKey { get; set; } = "Foreground";

        public void Initialize(AppTitleBar appTitleBar)
        {
            _appTitleBar = appTitleBar;

            if (LoadBackgroundSettings() is Color background)
            {
                SetBackground(background);
            }

            if (LoadForegroundSettings() is Color foreground)
            {
                SetForeground(foreground);
            }
        }

        public string? LoadTextSettings()
        {
            if (_settingsService.TryLoad(SettingsName, TextSettingsKey, out string? text) is true)
            {
                return text;
            }

            return null;
        }

        public bool SaveTextSettings(string text)
        {
            return _settingsService.TrySave(SettingsName, TextSettingsKey, text);
        }

        public string? GetText()
        {
            return _appTitleBar?.Text;
        }

        public void SetText(string text)
        {
            if (_appTitleBar is not null)
            {
                _appTitleBar.Text = text;
            }
        }

        public Color? LoadBackgroundSettings()
        {
            if (_settingsService.TryLoad(SettingsName, BackgroundSettingsKey, out Color? color) is true)
            {
                return color;
            }

            return null;
        }

        public bool SaveBackgroundSettings(Color color)
        {
            return _settingsService.TrySave(SettingsName, BackgroundSettingsKey, color);
        }

        public Color? GetBackground()
        {
            return (_appTitleBar?.Background is SolidColorBrush brush) ? brush.Color : null;
        }

        public void SetBackground(Color color)
        {
            if (_appTitleBar is not null)
            {
                _appTitleBar.Background = new SolidColorBrush(color);
            }
        }

        public Color? LoadForegroundSettings()
        {
            if (_settingsService.TryLoad(SettingsName, ForegroundSettingsKey, out Color? color) is true)
            {
                return color;
            }

            return null;
        }

        public bool SaveForegroundSettings(Color color)
        {
            return _settingsService.TrySave(SettingsName, ForegroundSettingsKey, color);
        }

        public Color? GetForeground()
        {
            return (_appTitleBar?.Foreground is SolidColorBrush brush) ? brush.Color : null;
        }

        public void SetForeground(Color color)
        {
            if (_appTitleBar is not null)
            {
                _appTitleBar.Foreground = new SolidColorBrush(color);
            }
        }
    }
}