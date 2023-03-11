using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.Storage;
using wingman.Interfaces;
using WinRT;

namespace wingman.Services
{

    public class MicrophoneDeviceService : IMicrophoneDeviceService, IDisposable
    {
        MicrophoneDevice? _currentMicrophoneDevice;
        public event EventHandler<double>? VolumeChanged;
        private AudioGraph? graph;
        private AudioFrameInputNode? _frameInputNode;
        private AudioFrameOutputNode? _frameOutputNode;
        private AudioDeviceInputNode? _deviceInputNode;

        private bool _isRecording = false;
        private AudioBuffer? _recBuffer;
        private AudioFileOutputNode? _audioFileOutputNode;
        private bool _disposing = false;

        private ulong _sampleRate;

        public async Task<IReadOnlyList<MicrophoneDevice>> GetMicrophoneDevicesAsync()
        {
            List<MicrophoneDevice> result = new();

            var devices = await DeviceInformation.FindAllAsync(DeviceClass.AudioCapture);

            foreach (var device in devices)
            {
                result.Add(new MicrophoneDevice
                {
                    Id = device.Id,
                    Name = device.Name,
                    Info = device
                });
            }

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _isRecording = false;

                if (_audioFileOutputNode != null) // this wont ever be true
                {
                    _audioFileOutputNode.Stop();
                    _audioFileOutputNode.Dispose();
                    _audioFileOutputNode = null;
                }

                if (graph != null)
                {
                    if (_frameOutputNode != null)
                    {
                        _frameOutputNode.Stop();
                        graph.QuantumStarted -= FrameOutputNode_QuantumStarted;
                        _frameOutputNode.Dispose();
                        _frameOutputNode = null;
                    }
                    if (_frameInputNode != null)
                    {
                        _frameInputNode.Stop();
                        _frameInputNode.Dispose();
                        _frameInputNode = null;
                    }
                    if (_deviceInputNode != null)
                    {
                        _deviceInputNode.Stop();
                        _deviceInputNode.Dispose();
                        _deviceInputNode = null;
                    }
                    graph.UnrecoverableErrorOccurred -= AudioGraph_UnrecoverableErrorOccurred;
                    graph.Stop();
                    graph.Dispose();
                    graph = null;
                }
                if (_recBuffer != null)
                {
                    _recBuffer.Dispose();
                    _recBuffer = null;
                }
            }
        }


        public void Dispose()
        {
            _disposing = true;
            Dispose(_disposing);
            GC.SuppressFinalize(this);
        }


        private void AudioGraph_UnrecoverableErrorOccurred(AudioGraph sender, AudioGraphUnrecoverableErrorOccurredEventArgs args)
        {
            if (sender == graph && args.Error != AudioGraphUnrecoverableError.None)
            {
                Debug.WriteLine("The audio graph encountered and unrecoverable error.");
                graph.Stop();
                graph.Dispose();
            }
        }

        public async Task SetMicrophoneDeviceAsync(MicrophoneDevice device)
        {
            Dispose(true);

            if (device != null)
            {
                _currentMicrophoneDevice = device;

                var settings = new AudioGraphSettings(AudioRenderCategory.Speech)
                {
                    EncodingProperties = AudioEncodingProperties.CreatePcm(16000, 1, 16),
                    QuantumSizeSelectionMode = QuantumSizeSelectionMode.LowestLatency,
                    //DesiredRenderDeviceAudioProcessing = Windows.Media.AudioProcessing.Raw
                };

                CreateAudioGraphResult? result = default(CreateAudioGraphResult);

                try
                {
                    result = await AudioGraph.CreateAsync(settings);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }


                if (result == null || result.Status != AudioGraphCreationStatus.Success)
                {
                    throw new Exception("AudioGraph creation error");
                }


                graph = result.Graph;
                graph.UnrecoverableErrorOccurred += AudioGraph_UnrecoverableErrorOccurred;

                var inputResult = await graph.CreateDeviceInputNodeAsync(MediaCategory.Speech, settings.EncodingProperties, device.Info);

                if (inputResult.Status != AudioDeviceNodeCreationStatus.Success)
                {
                    throw new Exception("AudioGraph input node creation error");
                }

                _deviceInputNode = inputResult.DeviceInputNode;
                //_deviceInputNode.OutgoingGain = 5;

                try
                {
                    _frameOutputNode = graph.CreateFrameOutputNode(settings.EncodingProperties);
                }
                catch (Exception)
                {
                }

                _deviceInputNode.AddOutgoingConnection(_frameOutputNode);

                _throttlecount = 0;
                graph.QuantumStarted += FrameOutputNode_QuantumStarted;

                graph.Start();
            }
        }

        [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        unsafe interface IMemoryBufferByteAccess
        {
            void GetBuffer(out byte* buffer, out uint capacity);
        }

        private long _throttlecount;
        private void FrameOutputNode_QuantumStarted(AudioGraph sender, object args)
        {
            if (_disposing)
                return;

            _throttlecount++;
            if (_throttlecount < 10)
                return;
            _throttlecount = 0;

            var frame = _frameOutputNode?.GetFrame() ?? default;
            if (frame == null) { return; }

            unsafe
            {
                using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Read))
                using (IMemoryBufferReference reference = buffer.CreateReference())
                {
                    double decibels = double.NegativeInfinity;
                    try
                    {

                        byte* dataInBytes;
                        uint capacityInBytes;
                        var memoryBuffer = reference.As<IMemoryBufferByteAccess>();
                        memoryBuffer.GetBuffer(out dataInBytes, out capacityInBytes);
                        int dataInFloatsLength = (int)capacityInBytes / sizeof(float);
                        float* dataInFloat = (float*)dataInBytes;

                        double sumOfSquares = 0.0;
                        for (int i = 0; i < capacityInBytes / sizeof(float); i++)
                        {
                            sumOfSquares += dataInFloat[i] * dataInFloat[i];
                        }

                        double rms = Math.Sqrt(sumOfSquares / (capacityInBytes / sizeof(float)));

                        decibels = 20 * Math.Log10(rms);

                    }
                    catch (Exception)
                    {

                    }

                    // Update UI with decibels value

                    if (Double.IsNaN(decibels))
                    {
                        decibels = 0;
                    }

                    double minDecibels = -150;
                    double maxDecibels = 100;
                    double normalizedVolume = (decibels - minDecibels) / (maxDecibels - minDecibels);

                    var volumePercentage = Math.Min(Math.Max(normalizedVolume * 100, 0), 100);

                    if (!_disposing)
                        VolumeChanged?.Invoke(this, volumePercentage);
                }
            }
        }

        private async Task<StorageFile> CreateTempFileOutputNode()
        {
            string uniqueId = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            string uniqueFileName = $"chatsample_{uniqueId}.mp3";

            Windows.Storage.StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile sampleFile = await temporaryFolder.CreateFileAsync(uniqueFileName, CreationCollisionOption.ReplaceExisting);

            var outProfile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);

            var outResult = await graph?.CreateFileOutputNodeAsync(sampleFile, outProfile);
            if (outResult?.Status != AudioFileNodeCreationStatus.Success)
            {
                throw new Exception("Couldn't create file output node.");
            }

            if (outResult?.Status != AudioFileNodeCreationStatus.Success)
            {
                throw new Exception("Couldn't create file output node.");
            }

            _audioFileOutputNode = outResult.FileOutputNode;

            if (_deviceInputNode == null)
            {
                throw new Exception("_deviceInputNode is null");
            }

            _deviceInputNode.AddOutgoingConnection(_audioFileOutputNode);



            return sampleFile;
        }
        StorageFile? tmpFile { get; set; } = null;

        public async Task StartRecording()
        {
            if (_isRecording || _currentMicrophoneDevice == null || graph == null)
            {
                return;
            }
            _isRecording = true;
            graph.Stop();
            tmpFile = await CreateTempFileOutputNode();
            graph.Start();
            if (_audioFileOutputNode == null)
            {
                throw new Exception("_audioFileOutputNode is null.");
            }
            _audioFileOutputNode.Start();
        }

        public async Task<StorageFile?> StopRecording()
        {
            if (!_isRecording)
            {
                return default(StorageFile);
            }

            if (_audioFileOutputNode == null)
            {
                throw new Exception("_audioFileOutputNode is null.");
            }
            _audioFileOutputNode.Stop();
            await _audioFileOutputNode.FinalizeAsync();

            _isRecording = false;
            return tmpFile;
        }
    }
}

