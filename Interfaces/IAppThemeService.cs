using Microsoft.UI.Xaml;
using System.Collections.Generic;

namespace wingman.Interfaces
{
    public interface IAppThemeService
    {
        IEnumerable<ElementTheme> AvailableThemes { get; }

        void Initialize(FrameworkElement rootElement);

        ElementTheme? LoadThemeSettings();

        bool SaveThemeSettings(ElementTheme theme);

        ElementTheme? GetTheme();

        void SetTheme(ElementTheme theme);
    }
}