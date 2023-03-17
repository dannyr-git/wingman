using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using wingman.ViewModels;

namespace wingman.Views
{
    public sealed partial class AudioInputControl : UserControl
    {
        public AudioInputControl()
        {
            InitializeComponent();
            ViewModel = Ioc.Default.GetRequiredService<AudioInputControlViewModel>();
            DataContext=ViewModel;
        }

        public AudioInputControlViewModel ViewModel { get; }
    }

}