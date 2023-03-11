using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using wingman.ViewModels;

namespace wingman.Views
{
    public sealed partial class ModalControl : UserControl
    {
        public ModalControl()
        {
            InitializeComponent();
            ViewModel = Ioc.Default.GetRequiredService<ModalControlViewModel>();
            DataContext = ViewModel;
        }

        public ModalControlViewModel ViewModel { get; }

    }
}