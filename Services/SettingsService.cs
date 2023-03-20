using OpenAI.GPT3.ObjectModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using Windows.ApplicationModel;
using wingman.Interfaces;

namespace wingman.Services
{
    public enum WingmanSettings
    {
        [Description("Current version of Wingman")]
        Version,
        [Description("Your OpenAI API Key")]
        ApiKey,
        [Description("Main Hotkey")]
        Main_Hotkey,
        [Description("Modal Hotkey")]
        Modal_Hotkey,
        [Description("Toggle: Trim Whitespaces from Response")]
        Trim_Whitespaces,
        [Description("Toggle: Trim Newlines from Response")]
        Trim_Newlines,
        [Description("Toggle: Append clipboard to main prompt.")]
        Append_Clipboard,
        [Description("Toggle: Append clipboard to modal prompt.")]
        Append_Clipboard_Modal,
        [Description("OpenAI System Pre-Prompt")]
        System_Preprompt,
        //[Description("OpenAI Model to Query")]
        //OpenAI_Model
    }

    public enum WingmanSupportedCompletionModels
    {
        ChatGPT = Models.Model.ChatGpt3_5Turbo,
        GPT4 = Models.Model.Gpt_4,
        GPT4_32K = Models.Model.Gpt_4_32k,
        Codex_Davinci = Models.Model.CodeDavinciV2,
    }

    public static class WingmanSettingsDefaults
    {
        public const string ApiKey = "sk-INSERTYOUROPENAIKEY";
        public const string Main_Hotkey = "Alt+`";
        public const string Modal_Hotkey = "`";
        public const string Trim_Whitespaces = "false";
        public const string Trim_Newlines = "false";
        public const string Append_Clipboard = "false";
        public const string Append_Clipboard_Modal = "false";
        public const string System_Preprompt = "You are a programming assistant. You are only allowed to respond with the raw code. Do not generate explanations. Do not preface. Do not follow-up after the code.";

        // Experiments
        public const string Whisper_Preprompt = "The following transcription will be contextually based around programming, coding, or scripting.  It might include technical language and programming terminology.";

    }


    public class SettingsService : ISettingsService
    {
        public readonly string SettingsFolderName = "Wingman";
        public readonly string SettingsFileName = "Wingman.settings";

        private readonly Dictionary<WingmanSettings, string> _settings; private readonly string _settingsFilePath;
        private readonly ILoggingService _loggingService;
        public SettingsService(ILoggingService loggingService)
        {
            _loggingService = loggingService; _settings = new Dictionary<WingmanSettings, string>();
            _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SettingsFolderName, SettingsFileName);

            if (!Directory.Exists(Path.GetDirectoryName(_settingsFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath));
            }

            if (!File.Exists(_settingsFilePath))
            {
                InitializeDefaultSettings();
                SaveSettings();
            }
            else
            {
                var json = File.ReadAllText(_settingsFilePath);
                if (json != null)
                {
                    try
                    {
                        _settings = JsonSerializer.Deserialize<Dictionary<WingmanSettings, string>>(json);
                    }
                    catch (JsonException)
                    {
                        _loggingService.LogWarning("Settings JSON is invalid, testing for old format...");
                        try
                        {
                            Dictionary<string, Dictionary<string, string>> oldSettings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);

                            _settings = new Dictionary<WingmanSettings, string>();

                            foreach (var pair in oldSettings["User"])
                            {
                                if (Enum.TryParse(pair.Key, out WingmanSettings key))
                                {
                                    _settings[key] = pair.Value;
                                }
                            }
                        }
                        catch (JsonException)
                        {
                            _loggingService.LogWarning("Settings not salvagable, reverting...");
                            _settings = null;
                        }
                    }

                    if (_settings != null && NeedsUpdate())
                    {
                        _loggingService.LogWarning("Old settings file detected, removing...");
                        File.Delete(_settingsFilePath);
                        _loggingService.LogWarning("Importing old values if possible...");
                        ImportExistingSettings(_settings);
                        EnsureAllSettingsPresent();
                        //InitializeDefaultSettings();
                        SaveSettings();
                    }
                    else
                    {
                        _loggingService.LogInfo("Sanitizing settings...");
                        EnsureAllSettingsPresent();
                    }
                }
                else
                {
                    _loggingService.LogError("Settings JSON is empty, reverting to defaults..."); InitializeDefaultSettings(); SaveSettings();
                }
            }
        }

        private void ImportExistingSettings(Dictionary<WingmanSettings, string> existingSettings)
        {
            foreach (var setting in existingSettings)
            {
                if (Enum.IsDefined(typeof(WingmanSettings), setting.Key))
                {
                    _settings[setting.Key] = setting.Value;
                }
            }
        }

        private bool NeedsUpdate()
        {
            var currentVersion = GetVersionFromAppxManifest();
            var savedVersion = Load<string>(WingmanSettings.Version);

            return savedVersion != currentVersion;
        }

        private string GetVersionFromAppxManifest()
        {
            var packageVersion = Package.Current.Id.Version;
            return $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
        }

        private void InitializeDefaultSettings()
        {
            foreach (WingmanSettings setting in Enum.GetValues(typeof(WingmanSettings)))
            {
                _settings[setting] = GetDefault(setting);
            }
        }

        private void EnsureAllSettingsPresent()
        {
            bool shouldSave = false;

            foreach (WingmanSettings setting in Enum.GetValues(typeof(WingmanSettings)))
            {
                if (!_settings.ContainsKey(setting))
                {
                    _loggingService.LogWarning(String.Format("Setting \"{0}\" missing from settings", Enum.GetName(setting)));
                    shouldSave = true;
                    _settings[setting] = GetDefault(setting);
                    _loggingService.LogWarning(String.Format("\"{0}\" set to \"{1}\"", Enum.GetName(setting), _settings[setting]));
                }
            }

            _settings[WingmanSettings.Version] = GetDefault(WingmanSettings.Version);

            if (shouldSave)
            {
                SaveSettings();
            }
        }

        private string GetDefault(WingmanSettings setting)
        {
            return setting switch
            {
                WingmanSettings.Version => GetVersionFromAppxManifest(),
                WingmanSettings.ApiKey => WingmanSettingsDefaults.ApiKey,
                WingmanSettings.Main_Hotkey => WingmanSettingsDefaults.Main_Hotkey,
                WingmanSettings.Modal_Hotkey => WingmanSettingsDefaults.Modal_Hotkey,
                WingmanSettings.Trim_Whitespaces => WingmanSettingsDefaults.Trim_Whitespaces,
                WingmanSettings.Trim_Newlines => WingmanSettingsDefaults.Trim_Newlines,
                WingmanSettings.Append_Clipboard => WingmanSettingsDefaults.Append_Clipboard,
                WingmanSettings.Append_Clipboard_Modal => WingmanSettingsDefaults.Append_Clipboard_Modal,
                WingmanSettings.System_Preprompt => WingmanSettingsDefaults.System_Preprompt,
                _ => throw new ArgumentOutOfRangeException(nameof(setting), setting, null)
            };
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

        public T? Load<T>(WingmanSettings key)
        {
            string keyString = Enum.GetName(typeof(WingmanSettings), key);
            if (_settings.TryGetValue(key, out var value))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            return default;
        }

        public bool TryLoad<T>(WingmanSettings key, out T? value)
        {
            value = Load<T>(key);

            return value != null;
        }

        public void Save<T>(WingmanSettings key, T value)
        {
            var stringValue = value?.ToString() ?? "";
            _settings[key] = stringValue;

            SaveSettings();
        }

        public bool TrySave<T>(WingmanSettings key, T value)
        {
            try
            {
                Save(key, value);
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.LogException($"Failed to save setting {Enum.GetName(typeof(WingmanSettings), key)}." + ex.ToString()); return false;
            }
        }
    }
}