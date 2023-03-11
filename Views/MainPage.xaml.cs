using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using wingman.ViewModels;

namespace wingman.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            ViewModel = Ioc.Default.GetRequiredService<MainPageViewModel>();
            DataContext = ViewModel;
        }

        public MainPageViewModel ViewModel { get; }

    }
}