using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using wingman.Interfaces;

namespace wingman.Services
{
    public class LocalSettingsService : ILocalSettings
    {
        private readonly Dictionary<string, Dictionary<string, string>> _settings;
        private readonly string _settingsFilePath;

        public LocalSettingsService()
        {
            _settings = new Dictionary<string, Dictionary<string, string>>();
            _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Wingman", "Wingman.settings");

            if (!Directory.Exists(Path.GetDirectoryName(_settingsFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath));
            }

            if (!File.Exists(_settingsFilePath))
            {
                SaveDefaults();
            }
            else
            {
                var json = File.ReadAllText(_settingsFilePath);
                _settings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
            }
        }

        private void SaveDefaults()
        {
            var defaultSettings = new Dictionary<string, string>
        {
            {"ApiKey", "sk-REPLACEWITHYOURKEY"},
            {"Main_Hotkey", "`"},
            {"Modal_Hotkey", "Alt+`"},
            {"Trim_Whitespaces", false.ToString()},
            {"Trim_Newlines", false.ToString()},
            {"Append_Clipboard", false.ToString()},
            {"Append_Clipboard_Modal", false.ToString()},
            {"System_Preprompt", "You are a programming assistant. You are only allowed to respond with the raw code. Do not generate explanations. Do not preface. Do not follow-up after the code."}
        };

            _settings["Default"] = defaultSettings;

            SaveSettings();
        }

        private void SaveSettings()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(_settings, options);
            File.WriteAllText(_settingsFilePath, json);
        }

        public T? Load<T>(string key)
        {
            if (_settings.TryGetValue("User", out var userSettings) &&
                userSettings.TryGetValue(key, out var value))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }

            if (_settings.TryGetValue("Default", out var defaultSettings) &&
                defaultSettings.TryGetValue(key, out var defaultValue))
            {
                return (T)Convert.ChangeType(defaultValue, typeof(T));
            }

            return default;
        }

        public bool TryLoad<T>(string key, out T? value)
        {
            value = Load<T>(key);

            return value != null;
        }

        public void Save<T>(string key, T value)
        {
            if (!_settings.ContainsKey("User"))
            {
                _settings["User"] = new Dictionary<string, string>();
            }

            var stringValue = value?.ToString() ?? "";

            _settings["User"][key] = stringValue;

            SaveSettings();
        }

        public bool TrySave<T>(string key, T value)
        {
            try
            {
                Save(key, value);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}