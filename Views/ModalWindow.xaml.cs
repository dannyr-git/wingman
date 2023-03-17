using Microsoft.UI.Xaml;
using wingman.Helpers;

namespace wingman.Views
{
    public sealed partial class ModalWindow : Window
    {
        public ModalWindow(string input, int width = 800, int height = 600, bool isResizable = true)
        {
            InitializeComponent();
            this.SetIsAlwaysOnTop(true);
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(ModalTitleBar);
            this.SetWindowSize(width, height);
            this.SetIsResizable(isResizable);
            myView.ViewModel.TextContent = input;
            AppTitleTextBlock.Text = this.Title;

            this.Activated += ModalWindow_Activated;
        }

        private void ModalWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            this.SetIsAlwaysOnTop(false);
        }
    }
}