using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Storage;

namespace wingman.Interfaces
{
    public interface IMicrophoneDeviceService
    {
        event EventHandler<double> VolumeChanged;
        Task<IReadOnlyList<MicrophoneDevice>> GetMicrophoneDevicesAsync();
        Task SetMicrophoneDeviceAsync(MicrophoneDevice device);
        Task StartRecording();
        Task<StorageFile?> StopRecording();
    }

    public class MicrophoneDevice
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public DeviceInformation? Info { get; set; }
    }
}
