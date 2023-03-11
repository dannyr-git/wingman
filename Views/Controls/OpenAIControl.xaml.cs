using CommunityToolkit.Mvvm.DependencyInjection;
using wingman.ViewModels;

namespace wingman.Views
{
    public sealed partial class OpenAIControl : Microsoft.UI.Xaml.Controls.UserControl
    {
        public OpenAIControl()
        {
            InitializeComponent();
            ViewModel = Ioc.Default.GetRequiredService<OpenAIControlViewModel>();
            DataContext = ViewModel;
        }

        public OpenAIControlViewModel ViewModel { get; }
    }
}

