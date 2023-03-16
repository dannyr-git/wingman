using Microsoft.UI.Xaml;
using wingman.Helpers;

namespace wingman.Views
{
    public sealed partial class ModalWindow : Window
    {
        public ModalWindow(string input, int width = 800, int height = 600, bool isResizable = true)
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(ModalTitleBar);
            this.SetWindowSize(width, height);
            this.SetIsResizable(isResizable);
            myView.ViewModel.TextContent = input;
        }
    }
}