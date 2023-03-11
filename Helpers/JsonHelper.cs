using System.Text.Json;
using System.Threading.Tasks;

namespace wingman.Helpers
{
    public static class JsonHelper
    {
        public static async Task<T?> DeserializeAsync<T>(string value) => await Task.Run(() => JsonSerializer.Deserialize<T>(value));

        public static async Task<string> SerializeAsync(object value) => await Task.Run(() => JsonSerializer.Serialize(value));
    }
}