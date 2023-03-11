using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using wingman.Helpers;
using wingman.ViewModels;

namespace wingman.Views
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBar);
            ViewModel = Ioc.Default.GetRequiredService<MainWindowViewModel>();

            this.SetWindowSize(800, 600);
            this.SetIsResizable(false);
        }

        public AppTitleBar TitleBar { get => AppTitleBar; }

        public MainWindowViewModel ViewModel { get; }
    }
}