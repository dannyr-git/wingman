using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using wingman.Interfaces;

namespace wingman.Services
{
    /*  These are my default values :

                settingsService.Save("Wingman", "Main_Hotkey", "`");
                settingsService.Save("Wingman", "Modal_Hotkey", "Ctrl+T");

                settingsService.Save("Wingman", "Trim_Whitespaces", false);
                settingsService.Save("Wingman", "Trim_Newlines", false);
                settingsService.Save("Wingman", "Append_Clipboard", false);
                settingsService.Save("Wingman", "Append_Clipboard_Modal", false);

                settingsService.Save("Wingman", "System_Preprompt", "You are a programming assistant.  You are only allowed to respond with the raw code.  Do not generate explanations.  Do not preface.  Do not follow-up after the code.");     
     */
    public class LocalSettingsService : ISettingsService
    {
        private Dictionary<string, Dictionary<string, string>> _settings;
        private readonly string _settingsFilePath;

        public LocalSettingsService()
        {
            _settings = new Dictionary<string, Dictionary<string, string>>();
            _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Wingman", "Wingman.settings");

            if (File.Exists(_settingsFilePath))
            {
                LoadSettingsFromFile();
            }
            else
            {
                CreateSettingsFileDirectory();
            }
        }

        public T? Load<T>(string containerName, string key)
        {
            Validate(containerName, key);

            if (_settings.TryGetValue(containerName, out var container) && container.TryGetValue(key, out var jsonString))
            {
                return JsonSerializer.Deserialize<T>(jsonString);
            }

            return default;
        }

        public bool TryLoad<T>(string containerName, string key, out T? value)
        {
            try
            {
                value = Load<T>(containerName, key);
                return true;
            }
            catch
            {
            }

            value = default;

            return false;
        }

        public void Save<T>(string containerName, string key, T value)
        {
            try
            {
                if (!_settings.TryGetValue(containerName, out var container))
                {
                    container = new Dictionary<string, string>();
                    _settings.Add(containerName, container);
                }

                container[key] = JsonSerializer.Serialize(value);

                SaveSettingsToFile();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save value to container '{containerName}' with key '{key}': {ex.Message}");
            }
        }

        public bool TrySave<T>(string containerName, string key, T value)
        {
            try
            {
                Save<T>(containerName, key, value);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save value to container '{containerName}' with key '{key}': {ex.Message}", ex);
            }
        }

        public void RemoveContainer(string containerName)
        {
            _settings.Remove(containerName);

            SaveSettingsToFile();
        }

        public void RemoveAllSettings()
        {
            _settings.Clear();

            SaveSettingsToFile();
        }

        private void Validate(string containerName, string key)
        {
            if (string.IsNullOrEmpty(containerName) is true)
            {
                throw new ContainerNameIsNullOrEmptyException(containerName);
            }

            if (string.IsNullOrEmpty(key) is true)
            {
                throw new KeyIsNullOrEmptyException(containerName, key);
            }

            if (_settings.ContainsKey(containerName) is false)
            {
                throw new ContainerNameDoesNotExistException(containerName);
            }

            if (_settings[containerName].ContainsKey(key) is false)
            {
                throw new KeyDoesNotExistException(containerName, key);
            }
        }

        private void LoadSettingsFromFile()
        {
            try
            {
                var settingsJson = File.ReadAllText(_settingsFilePath);
                _settings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(settingsJson);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load settings from file: {ex.Message}");
            }
        }

        private void SaveSettingsToFile()
        {
            try
            {
                var settingsJson = JsonSerializer.Serialize(_settings);
                File.WriteAllText(_settingsFilePath, settingsJson);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save settings to file: {ex.Message}");
            }
        }

        private void CreateSettingsFileDirectory()
        {
            try
            {
                var settingsFileDirectory = Path.GetDirectoryName(_settingsFilePath);
                if (!Directory.Exists(settingsFileDirectory))
                {
                    Directory.CreateDirectory(settingsFileDirectory);
                }

                if (!File.Exists(_settingsFilePath))
                {
                    var defaultSettings = new Dictionary<string, Dictionary<string, string>>()
                    {
                        {
                            // Default settings for the "Wingman" container
                            // TODO: Move these to a separate file, implement settings better
                            "Wingman", new Dictionary<string, string>()
                            {
                                {"ApiKey", "sk-REPLACEWITHYOURKEY"},
                                {"Main_Hotkey", "`"},
                                {"Modal_Hotkey", "Alt+`"},
                                {"Trim_Whitespaces", false.ToString()},
                                {"Trim_Newlines", false.ToString()},
                                {"Append_Clipboard", false.ToString()},
                                {"Append_Clipboard_Modal", false.ToString()},
                                {"System_Preprompt", "You are a programming assistant. You are only allowed to respond with the raw code. Do not generate explanations. Do not preface. Do not follow-up after the code."}
                            }
                        }
                    };

                    var settingsJson = JsonSerializer.Serialize(defaultSettings);
                    File.WriteAllText(_settingsFilePath, settingsJson);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create settings file directory: {ex.Message}");
            }
        }

        private void CreateDefaults()
        {
            // Not needed, as the default settings will be created by CreateSettingsFileDirectory()
        }


        public class LocalSettingsException : Exception
        {
            public LocalSettingsException(string containerName, string key = "")
            {
                ContainerName = containerName;
                Key = key;
            }

            public string ContainerName { get; }

            public string Key { get; }
        }

        public class ContainerNameIsNullOrEmptyException : LocalSettingsException
        {
            public ContainerNameIsNullOrEmptyException(string containerName) : base(containerName)
            {
            }
        }

        public class KeyIsNullOrEmptyException : LocalSettingsException
        {
            public KeyIsNullOrEmptyException(string containerName, string key) : base(containerName, key)
            {
            }
        }

        public class ContainerNameDoesNotExistException : LocalSettingsException
        {
            public ContainerNameDoesNotExistException(string containerName) : base(containerName)
            {
            }
        }

        public class KeyDoesNotExistException : LocalSettingsException
        {
            public KeyDoesNotExistException(string containerName, string key) : base(containerName, key)
            {
            }
        }

    }
}