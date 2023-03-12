using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.Media.Devices;

using Windows.UI.Core;
using wingman.Interfaces;

namespace wingman.ViewModels
{
    public class AudioInputControlViewModel : ObservableObject, IDisposable
    {
        private readonly IMicrophoneDeviceService _microphoneDeviceService;
        private readonly ISettingsService _settingsService;

        private List<MicrophoneDevice> _micDevices;
        private CoreDispatcher _dispatcher;
        private DispatcherQueue _dispatcherQueue;

        private List<string> _microphoneDeviceOptions = new List<string>();
        private double _progressBarValue;

        public ICommand RefreshDevices { get; }

        private string _selectedMicrophoneDevice;
        private bool _disposed = false;
        private bool _disposing = false;

        public AudioInputControlViewModel(IMicrophoneDeviceService microphoneDeviceService, ISettingsService settingsService)
        {
            _microphoneDeviceService = microphoneDeviceService;
            _settingsService = settingsService;

            _microphoneDeviceService.VolumeChanged += MicrophoneService_VolumeChanged;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            RefreshDevices = new RelayCommand(PopulateMicrophoneDeviceOptions);
            PopulateMicrophoneDeviceOptions();

            var chooseforme = MediaDevice.GetDefaultAudioCaptureId(AudioDeviceRole.Communications);
            if (!String.IsNullOrEmpty(chooseforme))
            {
                var chooser = _micDevices.FirstOrDefault(x => x.Info.Id.Equals(chooseforme));
                if (chooser != null)
                    SelectedMicrophoneDevice = chooser.Name;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _disposing = disposing;

            if (_disposed)
                return;

            if (disposing)
            {
                // Unsubscribe from event handlers
                if (_microphoneDeviceService != null)
                {
                    _microphoneDeviceService.VolumeChanged -= MicrophoneService_VolumeChanged;
                }

                // Dispose of any disposable objects



            }

            _disposed = true;
        }

        private void MicrophoneService_VolumeChanged(object sender, double volume)
        {
            if (!_disposing && !_disposed)
                _dispatcherQueue.TryEnqueue(() => { ProgressBarValue = volume; });

        }

        private async void PopulateMicrophoneDeviceOptions()
        {
            _micDevices = new List<MicrophoneDevice>();
            var devices = await _microphoneDeviceService.GetMicrophoneDevicesAsync();
            foreach (MicrophoneDevice dev in devices)
                _micDevices.Add(dev);
            _microphoneDeviceOptions = devices.Select(d => d.Name).ToList();
            OnPropertyChanged(nameof(MicrophoneDeviceOptions));
        }

        public double ProgressBarValue
        {
            get => _progressBarValue;
            set
            {
                try
                {
                    SetProperty(ref _progressBarValue, value);
                }
                catch (Exception)
                {
                    CoreApplication.Exit();
                }
            }
        }

        public List<string> MicrophoneDeviceOptions
        {
            get => _microphoneDeviceOptions;
            set => SetProperty(ref _microphoneDeviceOptions, value);
        }

        public string SelectedMicrophoneDevice
        {
            get => _selectedMicrophoneDevice;
            set
            {
                if (value != _selectedMicrophoneDevice)
                {
                    var newmic = _micDevices.Find(x => x.Name.Equals(value));
                    _microphoneDeviceService.SetMicrophoneDeviceAsync(newmic);
                    SetProperty(ref _selectedMicrophoneDevice, value);
                }
            }
        }

    }
}