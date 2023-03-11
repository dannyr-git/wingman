using System.Threading.Tasks;

namespace wingman.Interfaces
{
    public interface INamedPipesService
    {
        Task SendMessageAsync(string message);
    }
}
