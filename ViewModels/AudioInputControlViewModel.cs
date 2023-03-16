using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.Media.Devices;
using wingman.Interfaces;

namespace wingman.ViewModels
{
    public class AudioInputControlViewModel : ObservableObject, IDisposable
    {
        private readonly IMicrophoneDeviceService _microphoneDeviceService;
        private readonly ILocalSettings _settingsService;

        private List<MicrophoneDevice> _micDevices;
        private DispatcherQueue _dispatcherQueue;
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(100);
        private Stopwatch _stopwatch;
        private double _lastVolume;

        private List<string> _microphoneDeviceOptions = new List<string>();
        private EventHandler<double> MicrophoneService_VolumeChanged;
        private double _progressBarValue;

        public ICommand RefreshDevices { get; }

        private string _selectedMicrophoneDevice;
        private bool _disposed = false;
        private bool _disposing = false;

        public AudioInputControlViewModel(IMicrophoneDeviceService microphoneDeviceService, ILocalSettings settingsService)
        {
            _microphoneDeviceService = microphoneDeviceService;
            _settingsService = settingsService;

            MicrophoneService_VolumeChanged = async (sender, volume) =>
            {
                if (!_disposing && !_disposed)
                    await VolumeHandler(volume);
            };

            _microphoneDeviceService.VolumeChanged += MicrophoneService_VolumeChanged;

            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _stopwatch = Stopwatch.StartNew();

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

        private async Task VolumeHandler(double volume)
        {
            if (_disposing || _disposed) return;

            _lastVolume = volume;
            if (_stopwatch.Elapsed >= _updateInterval)
            {
                _stopwatch.Restart();
                await _dispatcherQueue.EnqueueAsync(() =>
                {
                    ProgressBarValue = _lastVolume;
                });
            }
        }

        private async void PopulateMicrophoneDeviceOptions()
        {
            _micDevices = new List<MicrophoneDevice>();
            var devices = await _microphoneDeviceService.GetMicrophoneDevicesAsync();

            if (devices == null || devices.Count == 0)
                return;

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