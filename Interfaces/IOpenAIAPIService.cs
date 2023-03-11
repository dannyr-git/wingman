using System.Threading.Tasks;
using Windows.Storage;

namespace wingman.Interfaces
{
    public interface IOpenAIAPIService
    {
        Task<string?> GetResponse(string prompt);
        Task<string?> GetWhisperResponse(StorageFile inmp3);
        Task<string?> GetApiKey();
        Task SetApiKey(string apiKey);
        Task<bool> IsValidKey();
    }
}
