using System.Threading.Tasks;

namespace wingman.Interfaces
{
    public interface IStdInService
    {
        Task SendWithClipboardAsync(string str);
    }
}
