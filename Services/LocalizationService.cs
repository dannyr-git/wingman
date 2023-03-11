using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Globalization;
using wingman.Interfaces;

namespace wingman.Services
{
    public class LocalizationService : ILocalizationService
    {
        private readonly ResourceManager _resourceManager;
        private readonly ResourceContext _resourceContext;

        public LocalizationService()
        {
            _resourceManager = new();
            _resourceContext = _resourceManager.CreateResourceContext();
            AvailableLanguages = ApplicationLanguages.ManifestLanguages;
            SettingsName = $"{GetType().Namespace}.{GetType().Name}";
        }

        public string SettingsName { get; set; }

        public string LanguageSettingsKey { get; set; } = "Language";

        public string DefaultLanguage { get; set; } = "en-US";

        public IEnumerable<string> AvailableLanguages { get; }

        public void Initialize()
        {
            string? language = GetLanguage();
            SetLanguage(language is not null && IsAvailable(language) ? language : DefaultLanguage);
        }

        public bool IsAvailable(string language)
        {
            return AvailableLanguages.Contains(language);
        }

        public string? GetLanguage()
        {
            string language = ApplicationLanguages.PrimaryLanguageOverride;
            return (IsAvailable(language) is true) ? language : null;
        }

        public void SetLanguage(string language)
        {
            if (AvailableLanguages.Contains(language) is true)
            {
                ApplicationLanguages.PrimaryLanguageOverride = language;
                _resourceContext.QualifierValues["Language"] = language;
                return;
            }

            throw new InvalidLanguageException(language, AvailableLanguages);
        }

        public class InvalidLanguageException : Exception
        {
            public InvalidLanguageException(string? language, IEnumerable<string> availableLanguages)
            {
                Language = language;
                AvailableLanguages = availableLanguages;
            }

            public string? Language { get; }

            public IEnumerable<string> AvailableLanguages { get; }
        }
    }
}