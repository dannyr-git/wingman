using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using wingman.ViewModels;

namespace wingman.Views
{
    public sealed partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();
            ViewModel = Ioc.Default.GetRequiredService<SettingsControlViewModel>();
            DataContext = ViewModel;
        }

        public SettingsControlViewModel ViewModel { get; }

    }
}