namespace wingman.Interfaces
{
    public interface ILocalSettings
    {
        T? Load<T>(string key);
        bool TryLoad<T>(string key, out T? value);
        void Save<T>(string key, T value);
        bool TrySave<T>(string key, T value);
    }
}