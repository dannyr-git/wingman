using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using wingman.Interfaces;
using wingman.Services;

namespace wingman.ViewModels
{
    public class SettingsControlViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;

        public SettingsControlViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            InitializeSettings();
        }

        private List<SettingViewModel> _settingsList;
        public List<SettingViewModel> SettingsList
        {
            get => _settingsList;
            set => SetProperty(ref _settingsList, value);
        }

        private void InitializeSettings()
        {
            var settings = new List<SettingViewModel>();

            foreach (WingmanSettings setting in Enum.GetValues(typeof(WingmanSettings)))
            {
                if (setting == WingmanSettings.Version)
                    continue;

                var description = setting.GetType().GetMember(setting.ToString())[0].GetCustomAttributes(typeof(DescriptionAttribute), false)[0] as DescriptionAttribute;
                var value = _settingsService.Load<string>(setting);

                settings.Add(new SettingViewModel(description.Description, value, setting, _settingsService));
            }

            SettingsList = settings;
        }

    }

    public class SettingViewModel : INotifyPropertyChanged
    {
        public string Description { get; }
        public object Editor { get; }

        private readonly ISettingsService _settingsService;
        private readonly WingmanSettings _setting;

        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                    _settingsService.Save(_setting, _value);
                }
            }
        }

        public SettingViewModel(string description, string value, WingmanSettings setting, ISettingsService settingsService)
        {
            Description = description;
            _setting = setting;
            _settingsService = settingsService;
            Value = value;

            if (setting == WingmanSettings.Trim_Whitespaces ||
                setting == WingmanSettings.Trim_Newlines ||
                setting == WingmanSettings.Append_Clipboard ||
                setting == WingmanSettings.Append_Clipboard_Modal)
            {
                Editor = new ComboBox
                {
                    Items = { "True", "False" },
                    SelectedItem = value
                };
            }
            else
            {
                Editor = new TextBox { Text = value, AcceptsReturn = true };
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
