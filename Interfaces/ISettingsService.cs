using wingman.Services;

namespace wingman.Interfaces
{
    public interface ISettingsService
    {
        T? Load<T>(WingmanSettings key);
        bool TryLoad<T>(WingmanSettings key, out T? value);
        void Save<T>(WingmanSettings key, T value);
        bool TrySave<T>(WingmanSettings key, T value);
    }
}