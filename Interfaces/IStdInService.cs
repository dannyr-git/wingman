using System.Diagnostics;
using System.Threading.Tasks;

namespace wingman.Interfaces
{
    public interface IStdInService
    {
        void SetProcess(Process process);
        Task SendStringAsync(string str);
    }
}
