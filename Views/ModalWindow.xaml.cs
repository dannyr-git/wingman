using Microsoft.UI.Xaml;
using wingman.Helpers;

namespace wingman.Views
{
    public sealed partial class ModalWindow : Window
    {
        public ModalWindow(string input)
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(ModalTitleBar);
            this.SetWindowSize(800, 600);
            //this.SetIsResizable(false);
            myView.ViewModel.TextContent = input;
        }
    }
}