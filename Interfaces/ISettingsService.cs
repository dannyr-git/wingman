namespace wingman.Interfaces
{
    public interface ISettingsService
    {
        T? Load<T>(string containerName, string key);

        bool TryLoad<T>(string containerName, string key, out T? value);

        void Save<T>(string containerName, string key, T value);

        bool TrySave<T>(string containerName, string key, T value);

        void RemoveAllSettings();

        void RemoveContainer(string containerName);
    }
}