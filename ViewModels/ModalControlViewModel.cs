using CommunityToolkit.Mvvm.ComponentModel;

namespace wingman.ViewModels
{
    public class ModalControlViewModel : ObservableObject
    {
        private string _textcontent;

        public string TextContent
        {
            get => _textcontent;
            set => SetProperty(ref _textcontent, value);
        }

        public ModalControlViewModel()
        {
        }
    }


}