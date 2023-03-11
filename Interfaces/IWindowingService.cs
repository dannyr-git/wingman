using Microsoft.UI.Xaml;

namespace wingman.Interfaces
{
    public interface IWindowingService
    {
        void Initialize(Window window);

        (int Width, int Height)? LoadWindowSizeSettings();

        bool SaveWindowSizeSettings(int width, int height);

        (int Width, int Height)? GetWindowSize();

        void SetWindowSize(int width, int height);

        bool? LoadIsAlwaysOnTopSettings();

        bool SaveIsAlwaysOnTopSettings(bool isAlwaysOnTop);

        bool? GetIsAlwaysOnTop();

        void SetIsAlwaysOnTop(bool isAlwaysOnTop);
    }
}