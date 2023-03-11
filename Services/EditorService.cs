using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using wingman.Interfaces;

namespace wingman.Services
{
    public class EditorService : IEditorService
    {
        public async Task<IReadOnlyList<Process>> GetRunningEditorsAsync()
        {
            var processes = await Task.Run(() => Process.GetProcesses());
            var editorProcesses = processes.Where(p => IsKnownEditorProcess(p)).ToList();
            return editorProcesses;
        }

        private bool IsKnownEditorProcess(Process process)
        {
            // Add known editor executable names here
            var knownEditorExes = new[] { "devenv", "code", "notepad++", "notepad", "sublime_text", "eclipse", "atom", "idea64", "pycharm64", "netbeans64", "brackets" };
            return knownEditorExes.Contains(Path.GetFileNameWithoutExtension(process.ProcessName));
        }
    }
}
