using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using wingman.Interfaces;


namespace wingman.ViewModels
{
    public class MainPageViewModel : ObservableObject
    {
        private readonly IEditorService _editorService;
        private readonly IStdInService _stdinService;
        private readonly ILocalSettings _settingsService;

        private List<string> _stdInTargetOptions = new List<string>();
        private List<Process> _runningProcesses;

        // Selected values
        private string _selectedStdInTarget;
        private string _preprompt;

        public MainPageViewModel(IEditorService editorService, IStdInService stdinService, ILocalSettings settingsService)
        {
            _editorService = editorService;
            _stdinService = stdinService;

            InitializeStdInTargetOptions();
            _settingsService = settingsService;
            PrePrompt = _settingsService.Load<string>("System_Preprompt");
        }

        public string PrePrompt
        {
            get => _preprompt;
            set
            {
                SetProperty(ref _preprompt, value);
                _settingsService.Save<string>("System_Preprompt", value);
            }
        }

        public List<string> StdInTargetOptions
        {
            get => _stdInTargetOptions;
            set => SetProperty(ref _stdInTargetOptions, value);
        }

        private string _chatGPTOutput;
        public string ChatGptOutput
        {
            get => _chatGPTOutput;
            set => SetProperty(ref _chatGPTOutput, value);
        }

        public string SelectedStdInTarget
        {
            get => _selectedStdInTarget;
            set
            {
                if (SetProperty(ref _selectedStdInTarget, value))
                {
                    // Find the process with the selected process name and ID
                    var selectedProcessName = _selectedStdInTarget.Substring(_selectedStdInTarget.IndexOf(']') + 2); // Extract the process name from the selected option
                    var selectedProcessId = int.Parse(_selectedStdInTarget.Substring(1, _selectedStdInTarget.IndexOf(']') - 1)); // Extract the process ID from the selected option
                    var selectedProcess = _runningProcesses.FirstOrDefault(p => p.ProcessName == selectedProcessName && p.Id == selectedProcessId);
                    if (selectedProcess != null)
                    {
                        // Set the process in the StdInService
                        _stdinService.SetProcess(selectedProcess);
                    }
                }
            }
        }

        private async void InitializeStdInTargetOptions()
        {

            _runningProcesses = (await _editorService.GetRunningEditorsAsync()).ToList();
            var distinctProcesses = _runningProcesses.GroupBy(p => new { p.ProcessName, p.Id }).Select(g => g.First());
            var targetOptions = distinctProcesses.Select(p => $"[{p.Id}] {p.ProcessName}");

            StdInTargetOptions = targetOptions.ToList();

        }

        public ICommand StartCommand => new RelayCommand(Start);

        private void Start()
        {
            // Logic to execute when Start button is clicked
        }

        public ICommand ActivateChatGPT => new AsyncRelayCommand(TestAsync);

        private async Task TestAsync()
        {

        }

    }
}