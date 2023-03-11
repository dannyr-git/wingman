using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using wingman.Interfaces;

namespace wingman.Services
{
    public class AppThemeService : IAppThemeService
    {
        private readonly ISettingsService _settingsService;
        private FrameworkElement? _rootElement;

        public AppThemeService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            SettingsName = $"{GetType().Namespace}.{GetType().Name}";
        }

        public string SettingsName { get; set; }

        public string ThemeNameSettingsKey { get; set; } = "ThemeName";

        public IEnumerable<ElementTheme> AvailableThemes { get => Enum.GetValues<ElementTheme>(); }

        public void Initialize(FrameworkElement rootElement)
        {
            _rootElement = rootElement;

            ElementTheme? theme = LoadThemeSettings();
            SetTheme(theme is not null ? theme.Value : ElementTheme.Default);
        }

        public ElementTheme? LoadThemeSettings()
        {
            if (_settingsService.TryLoad(SettingsName, ThemeNameSettingsKey, out string? themeName) is true)
            {
                if (Enum.TryParse(themeName, out ElementTheme theme) is true)
                {
                    return theme;
                }
            }

            return null;
        }

        public bool SaveThemeSettings(ElementTheme theme)
        {
            return _settingsService.TrySave(SettingsName, ThemeNameSettingsKey, Enum.GetName(theme));
        }

        public ElementTheme? GetTheme()
        {
            return _rootElement?.RequestedTheme;
        }

        public void SetTheme(ElementTheme theme)
        {
            if (_rootElement is not null)
            {
                _rootElement.RequestedTheme = theme;
            }
        }
    }
}